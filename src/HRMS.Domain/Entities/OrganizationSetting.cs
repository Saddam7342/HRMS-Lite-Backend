using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class OrganizationSetting : TenantEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = "string"; // string, int, bool, json
    public string? Description { get; set; }
    public bool IsEditable { get; set; } = true;
}

// Strongly typed settings models for JSON storage
public class LeavePolicySettings
{
    public int DefaultAnnualDays { get; set; } = 21;
    public int MaxLeavePerRequest { get; set; } = 15;
    public bool CarryForwardEnabled { get; set; } = true;
    public int MaxCarryForwardDays { get; set; } = 5;
    public bool GenderBasedPoliciesEnabled { get; set; } = false;
}

public class AttendanceSettings
{
    public TimeSpan CheckInStartTime { get; set; } = new(8, 0, 0);
    public int LateThresholdMinutes { get; set; } = 15;
    public bool AutoAbsentEnabled { get; set; } = true;
    public double MinWorkingHours { get; set; } = 8.0;
}

public class ExpenseSettings
{
    public decimal MaxClaimLimit { get; set; } = 5000;
    public decimal ReceiptRequiredThreshold { get; set; } = 100;
    public List<string> AllowedCategories { get; set; } = [];
}

public class TravelSettings
{
    public bool ApprovalRequired { get; set; } = true;
    public decimal BudgetMaxLimit { get; set; } = 10000;
    public int AdvanceBookingDays { get; set; } = 7;
}

public class OrganizationGeneralSettings
{
    public string WorkingHoursStart { get; set; } = "09:00";
    public string WorkingHoursEnd { get; set; } = "18:00";
    public string Timezone { get; set; } = "UTC";
    public List<DayOfWeek> Weekends { get; set; } = [DayOfWeek.Saturday, DayOfWeek.Sunday];
}
