namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Push/in-app notification service abstraction.
/// Implemented in Infrastructure (Firebase, SignalR, etc.)
/// </summary>
public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, object? data = null, CancellationToken cancellationToken = default);
    Task SendToTenantAsync(Guid tenantId, string title, string body, object? data = null, CancellationToken cancellationToken = default);
}
