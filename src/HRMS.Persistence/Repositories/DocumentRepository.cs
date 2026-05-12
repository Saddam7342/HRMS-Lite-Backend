using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class DocumentRepository(AppDbContext context) : GenericRepository<Document>(context), IDocumentRepository
{
    public async Task<IReadOnlyList<Document>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.EmployeeId == employeeId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetCompanyDocumentsAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.DocumentType == DocumentType.Company && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.Category == category && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
