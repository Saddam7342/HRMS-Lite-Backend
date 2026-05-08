namespace HRMS.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as auditable.
/// Ensures CreatedAt, UpdatedAt, CreatedBy, UpdatedBy are tracked.
/// Auto-populated by the AuditableEntityInterceptor in Persistence.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
}
