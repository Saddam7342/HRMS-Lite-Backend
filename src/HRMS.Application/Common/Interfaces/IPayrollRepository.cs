using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IPayrollRepository : IGenericRepository<Payroll>
{
    Task<Payroll?> GetByEmployeeAsync(Guid employeeId, int month, int year, CancellationToken ct = default);
    Task<IReadOnlyList<Payroll>> GetMonthlyPayrollAsync(Guid tenantId, int month, int year, CancellationToken ct = default);
    Task<SalaryStructure?> GetSalaryStructureAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<SalaryStructure>> GetAllSalaryStructuresAsync(Guid tenantId, CancellationToken ct = default);
}
