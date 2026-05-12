using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }

    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    // Approval tracking
    public Guid? ApprovedById { get; set; }
    public Employee? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
