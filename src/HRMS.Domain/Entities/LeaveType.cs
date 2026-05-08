using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class LeaveType : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DefaultDays { get; set; }
    
    public bool IsGenderSpecific { get; set; }
    public Gender? ApplicableGender { get; set; }
    
    public bool IsPaid { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}
