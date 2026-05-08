using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class AttendanceRecord : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal? TotalHours { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.CheckedIn;
    public bool IsLate { get; set; }
    public string? Notes { get; set; }
}
