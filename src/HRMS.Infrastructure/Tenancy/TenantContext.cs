using HRMS.Application.Common.Interfaces;

namespace HRMS.Infrastructure.Tenancy;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public bool IsResolved => TenantId != Guid.Empty;

    public void SetTenant(Guid tenantId, string? slug = null)
    {
        TenantId = tenantId;
        TenantSlug = slug;
    }
}
