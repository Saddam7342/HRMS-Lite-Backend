using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;
using FluentValidation;

namespace HRMS.Application.Features.Auth.Commands.Account;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}

public class AccountHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider) 
    : IRequestHandler<LogoutCommand, Result>,
      IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result.Failure("Unauthorized.");

        var user = await unitOfWork.Users.GetWithRefreshTokensAsync(userId.Value, cancellationToken);
        var refreshToken = user?.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);

        if (refreshToken != null)
        {
            refreshToken.RevokedAt = dateTimeProvider.UtcNow;
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success("Logged out successfully.");
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result.Failure("Unauthorized.");

        var user = await unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Result.Failure("Invalid current password.");

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetRequired = false;
        
        unitOfWork.Users.Update(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Password changed successfully.");
    }
}
