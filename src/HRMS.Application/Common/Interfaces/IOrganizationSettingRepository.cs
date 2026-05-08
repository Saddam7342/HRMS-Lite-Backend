using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IOrganizationSettingRepository : IGenericRepository<OrganizationSetting>
{
    Task<OrganizationSetting?> GetByKeyAsync(Guid tenantId, string key, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationSetting>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationSetting>> GetByModuleAsync(Guid tenantId, string modulePrefix, CancellationToken ct = default);
}
