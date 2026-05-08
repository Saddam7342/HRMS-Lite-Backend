using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class ExpenseClaim : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseClaimStatus Status { get; set; } = ExpenseClaimStatus.Pending;

    public string? ReceiptFileUrl { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Approval tracking
    public Guid? ApprovedById { get; set; }
    public Employee? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
