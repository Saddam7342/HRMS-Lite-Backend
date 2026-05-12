using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUsername, string Password, Guid? OrganizationId = null, string? Slug = null) : IRequest<Result<LoginResponse>>;

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
    IAuditService auditService,
    ITenantContext tenantContext) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var user = await unitOfWork.Users.GetWithRolesAndPermissionsAsync(request.EmailOrUsername, cancellationToken);

            if (user == null)
                return Result<LoginResponse>.Failure("Invalid credentials.");

            // Resolve OrganizationId from Slug if provided
            if (!string.IsNullOrEmpty(request.Slug))
            {
                var org = await unitOfWork.Organizations.GetBySlugAsync(request.Slug, cancellationToken);
                if (org == null || user.OrganizationId != org.Id)
                    return Result<LoginResponse>.Failure("Invalid credentials.");
            }
            else if (request.OrganizationId.HasValue && request.OrganizationId.Value != user.OrganizationId)
            {
                return Result<LoginResponse>.Failure("Invalid credentials.");
            }

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

            // We add the token but we'll use a safer way to commit
            user.RefreshTokens.Add(refreshToken);

            try 
            {
                // Use direct SQL to update the user to bypass any stealth global filters
                // The table name is "Users" based on migrations
                await unitOfWork.DbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE Users SET LastLoginAt = {0}, FailedLoginAttempts = 0, LockoutEnd = NULL WHERE Id = {1}",
                    user.LastLoginAt, user.Id, cancellationToken);
                
                // Save the refresh token separately
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
            }
            catch (Exception)
            {
                // Silently continue - don't block login if tracking fails
            }

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
                new TokenDto(accessToken, refreshTokenStr, refreshToken.ExpiresAt),
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
