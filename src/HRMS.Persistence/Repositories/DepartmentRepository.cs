using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class DepartmentRepository(AppDbContext context) : GenericRepository<Department>(context), IDepartmentRepository
{
    public async Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<Department?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.ParentDepartment)
            .Include(x => x.DepartmentHead)
            .Include(x => x.ChildDepartments)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Department>> GetHierarchyAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.DepartmentHead)
            .Where(x => x.IsActive)
            .ToListAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => x.Name == name, ct);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => x.Code == code, ct);

    public async Task<bool> HasEmployeesAsync(Guid departmentId, CancellationToken ct = default)
        => await _context.Employees.AnyAsync(x => x.DepartmentId == departmentId, ct);
}
