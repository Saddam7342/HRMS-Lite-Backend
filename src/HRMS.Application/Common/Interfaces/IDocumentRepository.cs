using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByOrganizationAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByCategoryAsync(Guid tenantId, string category, CancellationToken ct = default);
}
