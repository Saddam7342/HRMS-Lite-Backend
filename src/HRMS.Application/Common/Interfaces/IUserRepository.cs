using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IUserRepository : IGenericRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser?> GetWithRolesAndPermissionsAsync(string usernameOrEmail, CancellationToken ct = default);
    Task<AppUser?> GetWithRefreshTokensAsync(Guid userId, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
