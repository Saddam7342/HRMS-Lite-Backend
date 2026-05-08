using System.Security.Claims;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// JWT token generation and validation abstraction.
/// Implemented in Infrastructure. Application uses only this interface.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
