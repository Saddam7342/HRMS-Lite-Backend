namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Provides the identity context of the currently authenticated user.
/// Populated by CurrentUserService in Infrastructure from HttpContext claims.
/// Single-company HRMS — no TenantId.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Username { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
