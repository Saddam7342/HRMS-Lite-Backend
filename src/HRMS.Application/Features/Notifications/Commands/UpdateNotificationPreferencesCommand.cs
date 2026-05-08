using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Notifications.Commands;

public record UpdateNotificationPreferencesCommand(
    bool EmailEnabled,
    bool InAppEnabled,
    bool LeaveNotifications,
    bool ExpenseNotifications,
    bool TravelNotifications,
    bool AttendanceNotifications) : IRequest<Result>;

public class UpdateNotificationPreferencesHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateNotificationPreferencesCommand, Result>
{
    public async Task<Result> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result.Failure("Unauthorized.");

        var prefs = await unitOfWork.NotificationPreferences.GetByUserIdAsync(userId.Value, cancellationToken);
        
        if (prefs == null)
        {
            var user = await unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
            prefs = new NotificationPreferences
            {
                UserId = userId.Value,
                TenantId = user?.OrganizationId ?? Guid.Empty
            };
            await unitOfWork.NotificationPreferences.AddAsync(prefs, cancellationToken);
        }

        prefs.EmailEnabled = request.EmailEnabled;
        prefs.InAppEnabled = request.InAppEnabled;
        prefs.LeaveNotifications = request.LeaveNotifications;
        prefs.ExpenseNotifications = request.ExpenseNotifications;
        prefs.TravelNotifications = request.TravelNotifications;
        prefs.AttendanceNotifications = request.AttendanceNotifications;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
