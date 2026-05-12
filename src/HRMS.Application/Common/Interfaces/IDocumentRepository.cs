using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetCompanyDocumentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByCategoryAsync(string category, CancellationToken ct = default);
}
