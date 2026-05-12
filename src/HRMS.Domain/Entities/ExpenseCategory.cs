using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class ExpenseCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ExpenseClaim> ExpenseClaims { get; set; } = [];
}
