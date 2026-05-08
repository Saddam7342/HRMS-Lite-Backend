using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class PayrollRepository(AppDbContext context) : GenericRepository<Payroll>(context), IPayrollRepository
{
    public async Task<Payroll?> GetByEmployeeAsync(Guid employeeId, int month, int year, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Month == month && x.Year == year, ct);
    }

    public async Task<IReadOnlyList<Payroll>> GetMonthlyPayrollAsync(Guid tenantId, int month, int year, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Where(x => x.TenantId == tenantId && x.Month == month && x.Year == year)
            .ToListAsync(ct);
    }

    public async Task<SalaryStructure?> GetSalaryStructureAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await context.Set<SalaryStructure>()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, ct);
    }

    public async Task<IReadOnlyList<SalaryStructure>> GetAllSalaryStructuresAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await context.Set<SalaryStructure>()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(ct);
    }
}
