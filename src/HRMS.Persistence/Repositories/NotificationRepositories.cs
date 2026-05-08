using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class NotificationRepository(AppDbContext context) : GenericRepository<Notification>(context), INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(x => x.UserId == userId && !x.IsRead, ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await _dbSet.Where(x => x.UserId == userId && !x.IsRead).ToListAsync(ct);
        foreach (var n in unread)
        {
            n.IsRead = true;
        }
    }
}

public class NotificationPreferencesRepository(AppDbContext context) : GenericRepository<NotificationPreferences>(context), INotificationPreferencesRepository
{
    public async Task<NotificationPreferences?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId, ct);
    }
}
