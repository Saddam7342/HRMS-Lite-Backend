using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class AuditLogRepository(AppDbContext context) : GenericRepository<AuditLog>(context), IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLog>> GetEntityHistoryAsync(string entityName, string entityId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.EntityName == entityName && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetUserActivityAsync(Guid userId, int limit = 50, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetSystemLogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _dbSet
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
