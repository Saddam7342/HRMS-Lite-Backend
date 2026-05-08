using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Constants;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<TokenDto>>;

public class RefreshTokenHandler(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<RefreshTokenCommand, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdStr = principal?.FindFirst(AppClaimTypes.UserId)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            return Result<TokenDto>.Failure("Invalid token.");

        var user = await unitOfWork.Users.GetWithRefreshTokensAsync(userId, cancellationToken);
        if (user == null) return Result<TokenDto>.Failure("User not found.");

        var existingRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);

        if (existingRefreshToken == null || !existingRefreshToken.IsActive)
            return Result<TokenDto>.Failure("Invalid refresh token.");

        // Rotate Refresh Token
        var newRefreshTokenStr = jwtTokenService.GenerateRefreshToken();
        var newAccessToken = jwtTokenService.GenerateAccessToken(user);

        existingRefreshToken.RevokedAt = dateTimeProvider.UtcNow;
        existingRefreshToken.ReplacedByToken = newRefreshTokenStr;

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = dateTimeProvider.UtcNow
        };

        user.RefreshTokens.Add(newRefreshToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<TokenDto>.Success(new TokenDto(newAccessToken, newRefreshTokenStr, newRefreshToken.ExpiresAt));
    }
}
