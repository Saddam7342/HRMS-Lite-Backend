using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetEntityHistoryAsync(string entityName, string entityId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetUserActivityAsync(Guid userId, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetSystemLogsAsync(int page, int pageSize, CancellationToken ct = default);
}
