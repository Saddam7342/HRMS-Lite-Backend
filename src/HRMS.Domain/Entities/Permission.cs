using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents a granular permission in the system.
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;    // e.g. "employees:create"
    public string Module { get; set; } = string.Empty;  // e.g. "Employees"

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
