using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public enum PayrollStatus
{
    Draft = 1,
    Generated = 2,
    Approved = 3,
    Paid = 4
}

public class SalaryStructure : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public decimal BasicSalary { get; set; }
    public string Allowances { get; set; } = "[]"; // JSON: List<AllowanceModel>
    public string Deductions { get; set; } = "[]"; // JSON: List<DeductionModel>
    public decimal OvertimeRatePerHour { get; set; }
    
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class Payroll : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public int Month { get; set; }
    public int Year { get; set; }
    
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime GeneratedAt { get; set; }
    
    public Guid? ApprovedById { get; set; }
    public AppUser? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string AllowanceBreakdown { get; set; } = "[]"; // JSON details for payslip
    public string DeductionBreakdown { get; set; } = "[]"; // JSON details for payslip
}

// Models for JSON storage
public class AllowanceModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DeductionModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}
