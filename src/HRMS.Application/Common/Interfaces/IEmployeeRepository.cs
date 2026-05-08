using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;

namespace HRMS.Application.Common.Interfaces;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Employee?> GetByEmployeeCodeAsync(string code, CancellationToken ct = default);
    Task<Employee?> GetWithUserAndDepartmentAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid tenantId, CancellationToken ct = default);
    Task<int> GetCountByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
