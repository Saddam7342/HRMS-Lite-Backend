using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class UserRepository(AppDbContext context) : GenericRepository<AppUser>(context), IUserRepository
{
    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Username == username, ct);
    }

    public async Task<AppUser?> GetWithRolesAndPermissionsAsync(string usernameOrEmail, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => (x.Username == usernameOrEmail || x.Email == usernameOrEmail), ct);
    }

    public async Task<AppUser?> GetWithRefreshTokensAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(x => x.Email == email, ct);
    }
}
