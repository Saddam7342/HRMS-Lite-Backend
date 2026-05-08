using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class LeaveBalance : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays => TotalDays - UsedDays;
    
    public int Year { get; set; }
}
