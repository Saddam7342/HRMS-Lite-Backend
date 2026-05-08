using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}

public interface INotificationPreferencesRepository : IGenericRepository<NotificationPreferences>
{
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
