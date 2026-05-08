using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class OrganizationSettingRepository(AppDbContext context) : GenericRepository<OrganizationSetting>(context), IOrganizationSettingRepository
{
    public async Task<OrganizationSetting?> GetByKeyAsync(Guid tenantId, string key, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key, ct);
    }

    public async Task<IReadOnlyList<OrganizationSetting>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrganizationSetting>> GetByModuleAsync(Guid tenantId, string modulePrefix, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.TenantId == tenantId && x.Key.StartsWith(modulePrefix))
            .ToListAsync(ct);
    }
}
