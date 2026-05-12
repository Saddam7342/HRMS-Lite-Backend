using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class TravelRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public TravelRequestStatus Status { get; set; } = TravelRequestStatus.Pending;

    public decimal? EstimatedBudget { get; set; }

    // Approval tracking
    public Guid? ApprovedById { get; set; }
    public Employee? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
