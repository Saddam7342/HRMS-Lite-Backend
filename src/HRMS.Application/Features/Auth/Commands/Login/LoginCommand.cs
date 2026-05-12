using FluentValidation;
using Microsoft.EntityFrameworkCore;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

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
    IAuditService auditService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var user = await unitOfWork.Users.GetWithRolesAndPermissionsAsync(request.EmailOrUsername, cancellationToken);

            if (user == null)
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

            var accessToken = jwtTokenService.GenerateAccessToken(user);
            var refreshTokenStr = jwtTokenService.GenerateRefreshToken();

            var refreshTokenExpires = dateTimeProvider.UtcNow.AddDays(7);

            var roles = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(r => r.Role.Name)
                .ToList();

            var permissions = user.UserRoles
                .Where(ur => ur.Role != null)
                .SelectMany(r => r.Role.RolePermissions)
                .Where(rp => rp.Permission != null)
                .Select(p => p.Permission.Code)
                .Distinct()
                .ToList();

            var response = new LoginResponse(
                user.Id,
                user.Email,
                $"{user.FirstName} {user.LastName}",
                new TokenDto(accessToken, refreshTokenStr, refreshTokenExpires),
                roles,
                permissions
            );

            return Result<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure($"Internal Login Error: {ex.Message}");
        }
    }
}
