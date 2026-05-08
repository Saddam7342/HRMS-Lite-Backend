using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IPayrollEngine
{
    Task<Payroll> CalculateMonthlyPayrollAsync(SalaryStructure structure, int month, int year, CancellationToken ct = default);
}
