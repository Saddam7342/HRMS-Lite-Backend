using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    string? RelatedEntityId,
    string? RelatedEntityType,
    DateTime CreatedAt);

public record NotificationPreferencesDto(
    bool EmailEnabled,
    bool InAppEnabled,
    bool LeaveNotifications,
    bool ExpenseNotifications,
    bool TravelNotifications,
    bool AttendanceNotifications);

public record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool InAppEnabled,
    bool LeaveNotifications,
    bool ExpenseNotifications,
    bool TravelNotifications,
    bool AttendanceNotifications);
