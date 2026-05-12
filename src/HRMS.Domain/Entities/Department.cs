using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Hierarchy
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = [];

    // Leadership
    public Guid? DepartmentHeadId { get; set; }
    public Employee? DepartmentHead { get; set; }

    // Employees
    public ICollection<Employee> Employees { get; set; } = [];
}
