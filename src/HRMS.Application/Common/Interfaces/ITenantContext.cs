namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Provides the resolved tenant context for the current request.
/// Populated by TenantResolutionMiddleware in the API layer.
/// Injected into DbContext to apply global query filters.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }
    bool IsResolved { get; }
    void SetTenant(Guid tenantId, string? slug = null);
}
