using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services;

public class PayrollEngine(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : IPayrollEngine
{
    public async Task<Payroll> CalculateMonthlyPayrollAsync(SalaryStructure structure, int month, int year, CancellationToken ct = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // 1. Parse Structure
        var allowances = JsonSerializer.Deserialize<List<AllowanceModel>>(structure.Allowances) ?? [];
        var deductions = JsonSerializer.Deserialize<List<DeductionModel>>(structure.Deductions) ?? [];

        var totalAllowances = allowances.Sum(x => x.Amount);
        var baseDeductions = deductions.Sum(x => x.Amount);

        // 2. Fetch Unpaid Leaves (Penalty)
        var unpaidLeaves = await unitOfWork.LeaveRequests.GetQueryable()
            .Where(x => x.EmployeeId == structure.EmployeeId && 
                        x.Status == LeaveRequestStatus.Approved &&
                        x.LeaveType.Name.Contains("Unpaid") && 
                        x.StartDate >= startDate && x.EndDate <= endDate)
            .SumAsync(x => x.TotalDays, ct);

        decimal leavePenalty = 0;
        if (unpaidLeaves > 0)
        {
            var dailyRate = structure.BasicSalary / 30; // Simplified 30-day month
            leavePenalty = (decimal)unpaidLeaves * dailyRate;
            deductions.Add(new DeductionModel { Name = "Unpaid Leave", Amount = leavePenalty, Reason = $"{unpaidLeaves} days" });
        }

        // 3. Attendance Deductions (Late/Absent)
        var attendance = await unitOfWork.Attendance.GetQueryable()
            .Where(x => x.EmployeeId == structure.EmployeeId && x.Date >= startDate && x.Date <= endDate)
            .ToListAsync(ct);

        var lates = attendance.Count(x => x.IsLate);
        decimal lateDeduction = 0;
        if (lates > 3) // Policy: 3 lates allowed, then penalty
        {
            lateDeduction = (lates - 3) * 50; // Example fixed penalty
            deductions.Add(new DeductionModel { Name = "Late Arrival Penalty", Amount = lateDeduction, Reason = $"{lates} lates" });
        }

        var totalDeductions = baseDeductions + leavePenalty + lateDeduction;

        // 4. Create Record
        return new Payroll
        {
            EmployeeId = structure.EmployeeId,
            TenantId = structure.TenantId,
            Month = month,
            Year = year,
            BasicSalary = structure.BasicSalary,
            TotalAllowances = totalAllowances,
            TotalDeductions = totalDeductions,
            NetSalary = structure.BasicSalary + totalAllowances - totalDeductions,
            Status = PayrollStatus.Generated,
            GeneratedAt = dateTimeProvider.UtcNow,
            AllowanceBreakdown = JsonSerializer.Serialize(allowances),
            DeductionBreakdown = JsonSerializer.Serialize(deductions)
        };
    }
}
