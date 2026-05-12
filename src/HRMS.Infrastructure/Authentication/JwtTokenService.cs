using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Settings;
using HRMS.Shared.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.Infrastructure.Authentication;

/// <summary>
/// Generates JWT access tokens and refresh tokens.
/// Single-company HRMS — no TenantId claim included.
/// Claims: UserId, Email, Username, Roles, Permissions.
/// </summary>
public class JwtTokenService(IOptions<JwtSettings> jwtSettings) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public string GenerateAccessToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(AppClaimTypes.Username,         user.Username),
            new(AppClaimTypes.UserId,           user.Id.ToString()),
            new(AppClaimTypes.FullName,         $"{user.FirstName} {user.LastName}".Trim())
        };

        // Roles & Permissions from UserRoles
        foreach (var userRole in user.UserRoles)
        {
            if (userRole.Role == null) continue;
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

            foreach (var rolePermission in userRole.Role.RolePermissions)
            {
                if (rolePermission.Permission == null) continue;
                claims.Add(new Claim(AppClaimTypes.Permission, rolePermission.Permission.Code));
            }
        }

        var secret = !string.IsNullOrWhiteSpace(_jwtSettings.Secret)
            ? _jwtSettings.Secret
            : "SuperSecretDefaultKeyForDevelopment_AtLeast32CharsLong!";

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience         = false,
            ValidateIssuer           = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime         = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal    = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
