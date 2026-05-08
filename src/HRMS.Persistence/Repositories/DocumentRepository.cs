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

    public async Task<IReadOnlyList<Document>> GetByOrganizationAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.TenantId == tenantId && x.DocumentType == DocumentType.Organization && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetByCategoryAsync(Guid tenantId, string category, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.TenantId == tenantId && x.Category == category && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
