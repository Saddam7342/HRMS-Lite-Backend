using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Settings;
using HRMS.Shared.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.Infrastructure.Authentication;

/// <summary>
/// Generates JWT access tokens and refresh tokens.
/// Simplified to use direct IConfiguration for maximum reliability in production environments.
/// </summary>
public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
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

        var secret   = configuration["JwtSettings:Secret"]  ?? "SuperSecretDefaultKeyForDevelopment_AtLeast32CharsLong!";
        var issuer   = configuration["JwtSettings:Issuer"]  ?? "HRMS";
        var audience = configuration["JwtSettings:Audience"] ?? "HRMS-Clients";
        var expiry   = int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var min) ? min : 60;

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
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
        var secret = configuration["JwtSettings:Secret"] ?? "SuperSecretDefaultKeyForDevelopment_AtLeast32CharsLong!";

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience         = false,
            ValidateIssuer           = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
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
