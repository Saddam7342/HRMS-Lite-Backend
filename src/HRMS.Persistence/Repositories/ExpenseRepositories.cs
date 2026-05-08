using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class ExpenseCategoryRepository(AppDbContext context) : GenericRepository<ExpenseCategory>(context), IExpenseCategoryRepository
{
    public async Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _dbSet.Where(x => x.IsActive && x.TenantId == tenantId).ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid tenantId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(x => x.Code == code && x.TenantId == tenantId, ct);
    }
}

public class ExpenseClaimRepository(AppDbContext context) : GenericRepository<ExpenseClaim>(context), IExpenseClaimRepository
{
    public async Task<IReadOnlyList<ExpenseClaim>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Category)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.ExpenseDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExpenseClaim>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.Category)
            .Where(x => x.Employee.ManagerId == managerId && x.Status == ExpenseClaimStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExpenseClaim>> GetTeamClaimsAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.Category)
            .Where(x => x.Employee.ManagerId == managerId || x.EmployeeId == managerId)
            .OrderByDescending(x => x.ExpenseDate)
            .ToListAsync(ct);
    }

    public async Task<ExpenseClaim?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.Category)
            .Include(x => x.ApprovedBy)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
