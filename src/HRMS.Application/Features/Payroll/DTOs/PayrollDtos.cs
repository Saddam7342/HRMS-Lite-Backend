using HRMS.Domain.Entities;

namespace HRMS.Application.Features.Payroll.DTOs;

public record SalaryStructureDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    decimal BasicSalary,
    List<AllowanceModel> Allowances,
    List<DeductionModel> Deductions,
    decimal OvertimeRatePerHour,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo);

public record PayrollDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal BasicSalary,
    decimal TotalAllowances,
    decimal TotalDeductions,
    decimal NetSalary,
    PayrollStatus Status,
    DateTime GeneratedAt,
    string? ApproverName,
    DateTime? ApprovedAt);

public record PayrollSummaryDto(
    int TotalEmployees,
    decimal TotalBasicSalary,
    decimal TotalAllowances,
    decimal TotalDeductions,
    decimal TotalNetSalary,
    int ApprovedCount,
    int PendingCount);

public record PaySlipDto(
    Guid PayrollId,
    string EmployeeName,
    string Designation,
    string Department,
    int Month,
    int Year,
    decimal BasicSalary,
    List<AllowanceModel> Allowances,
    List<DeductionModel> Deductions,
    decimal NetSalary,
    DateTime GeneratedAt);

public record CreateSalaryStructureRequest(
    Guid EmployeeId,
    decimal BasicSalary,
    List<AllowanceModel> Allowances,
    List<DeductionModel> Deductions,
    decimal OvertimeRatePerHour,
    DateTime EffectiveFrom);
