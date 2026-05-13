using FluentValidation;
using HRMS.Application.Common.Configuration;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRMS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUsername, string Password) : IRequest<Result<LoginResponse>>;

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
    IOptions<AuthSettings> authSettings,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetWithRolesAndPermissionsAsync(request.EmailOrUsername, cancellationToken);

        if (user == null)
            return Result<LoginResponse>.Failure("Invalid credentials.");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is deactivated.");

        var bypass = authSettings.Value.DevLoginBypassPassword;
        var useDevBypass = !string.IsNullOrEmpty(bypass) &&
                           string.Equals(request.Password, bypass, StringComparison.Ordinal);

        if (!useDevBypass && user.LockoutEnd > dateTimeProvider.UtcNow)
            return Result<LoginResponse>.Failure("Account is locked due to multiple failed attempts.");

        if (!useDevBypass && !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
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

        if (useDevBypass)
            logger.LogWarning("Dev login bypass used for user {UserId} ({Email}).", user.Id, user.Email);

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenStr = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpires = dateTimeProvider.UtcNow.AddDays(7);

        unitOfWork.DbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = refreshTokenExpires,
            CreatedAt = dateTimeProvider.UtcNow
        });

        var roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(r => r.Role!.Name)
            .ToList();

        var permissions = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(r => r.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(p => p.Permission!.Code)
            .Distinct()
            .ToList();

        await unitOfWork.CommitAsync(cancellationToken);

        var response = new LoginResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            new TokenDto(accessToken, refreshTokenStr, refreshTokenExpires),
            roles,
            permissions
        );

        return Result<LoginResponse>.Success(response, "Login successful.");
    }
}
