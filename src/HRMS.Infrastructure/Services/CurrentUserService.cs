using System.Security.Claims;
using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.Services;

/// <summary>
/// Reads the current user's identity from JWT claims.
/// Single-company HRMS — no TenantId claim.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId => GetGuidClaim(AppClaimTypes.UserId);
    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);
    public string? Username => _user?.FindFirstValue(AppClaimTypes.Username);

    public IReadOnlyList<string> Roles => _user?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value).ToList() ?? [];

    public IReadOnlyList<string> Permissions => _user?.FindAll(AppClaimTypes.Permission)
        .Select(c => c.Value).ToList() ?? [];

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    private Guid? GetGuidClaim(string type)
    {
        var value = _user?.FindFirstValue(type);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
