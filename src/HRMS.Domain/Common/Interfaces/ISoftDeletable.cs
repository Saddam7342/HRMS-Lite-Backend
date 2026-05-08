namespace HRMS.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as soft-deletable.
/// The SoftDeleteInterceptor in Persistence intercepts Delete operations
/// and sets IsDeleted = true instead of issuing a DELETE statement.
/// Global query filters automatically exclude soft-deleted records.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
