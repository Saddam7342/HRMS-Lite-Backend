using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class EmployeeRepository(AppDbContext context) : GenericRepository<Employee>(context), IEmployeeRepository
{
    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task<Employee?> GetByEmployeeCodeAsync(string code, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.EmployeeCode == code, ct);

    public async Task<Employee?> GetWithUserAndDepartmentAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.User)
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await _dbSet.Where(x => x.DepartmentId == departmentId).ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default)
        => await _dbSet.Where(x => x.ManagerId == managerId).ToListAsync(ct);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => x.EmployeeCode == code, ct);

    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
        => await _dbSet.CountAsync(x => x.IsActive, ct);
}
