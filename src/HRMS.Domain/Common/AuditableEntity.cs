using HRMS.Domain.Common.Interfaces;

namespace HRMS.Domain.Common;

/// <summary>
/// Base entity with full audit trail and soft-delete support.
/// Most system entities (employees, leaves, etc.) inherit from this.
/// Audit fields are auto-populated by AuditableEntityInterceptor.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
