using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUsername, string Password, Guid? OrganizationId = null) : IRequest<Result<LoginResponse>>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginHandler(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetWithRolesAndPermissionsAsync(request.EmailOrUsername, cancellationToken);

        if (user == null)
            return Result<LoginResponse>.Failure("Invalid credentials.");

        // If OrganizationId is provided, ensure it matches the user's organization
        if (request.OrganizationId.HasValue && request.OrganizationId.Value != user.OrganizationId)
            return Result<LoginResponse>.Failure("Invalid credentials.");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is deactivated.");

        if (user.LockoutEnd > dateTimeProvider.UtcNow)
            return Result<LoginResponse>.Failure("Account is locked due to multiple failed attempts.");

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = dateTimeProvider.UtcNow.AddMinutes(30);
                user.FailedLoginAttempts = 0;
            }
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<LoginResponse>.Failure("Invalid credentials.");
        }

        // Reset tracking on successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = dateTimeProvider.UtcNow;

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenStr = jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = dateTimeProvider.UtcNow
        };

        user.RefreshTokens.Add(refreshToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // Audit Login
        await auditService.LogActivityAsync(
            AuditActionType.Login, 
            "AppUser", 
            user.Id.ToString(), 
            "User logged in successfully.", 
            null, 
            new { user.Email, user.LastLoginAt }, 
            user.OrganizationId,
            cancellationToken);

        var response = new LoginResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            new TokenDto(accessToken, refreshTokenStr, refreshToken.ExpiresAt),
            user.UserRoles.Select(r => r.Role.Name).ToList(),
            user.UserRoles.SelectMany(r => r.Role.RolePermissions).Select(p => p.Permission.Code).Distinct().ToList()
        );

        return Result<LoginResponse>.Success(response);
    }
}
