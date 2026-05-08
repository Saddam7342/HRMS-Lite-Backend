using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class NotificationPreferences : TenantEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    
    public bool LeaveNotifications { get; set; } = true;
    public bool ExpenseNotifications { get; set; } = true;
    public bool TravelNotifications { get; set; } = true;
    public bool AttendanceNotifications { get; set; } = true;
}
