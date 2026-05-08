namespace HRMS.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as tenant-scoped.
/// EF Core global query filter in AppDbContext will automatically
/// filter all queries by TenantId to prevent cross-tenant data leaks.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
