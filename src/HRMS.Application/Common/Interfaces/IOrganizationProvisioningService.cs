using HRMS.Shared.Models;

namespace HRMS.Application.Common.Interfaces;

public interface IOrganizationProvisioningService
{
    /// <summary>
    /// Provisions a new organization with an admin user and default settings.
    /// </summary>
    Task<Result<Guid>> ProvisionOrganizationAsync(
        string name, 
        string slug, 
        string adminEmail, 
        int maxEmployeeSlots, 
        CancellationToken ct = default);
}
