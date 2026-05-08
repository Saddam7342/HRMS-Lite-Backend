using HRMS.Domain.Common.Interfaces;

namespace HRMS.Domain.Common;

/// <summary>
/// Base entity for all tenant-scoped data.
/// Inherits full audit trail + soft delete.
/// TenantId is stamped automatically by the DbContext on save.
/// The EF Core global query filter ensures no query ever crosses tenant boundaries.
/// </summary>
public abstract class TenantEntity : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
}
