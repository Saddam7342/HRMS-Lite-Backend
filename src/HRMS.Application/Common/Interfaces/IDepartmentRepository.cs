using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Department?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Department>> GetHierarchyAsync(CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid tenantId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid tenantId, CancellationToken ct = default);
    Task<bool> HasEmployeesAsync(Guid departmentId, CancellationToken ct = default);
}
