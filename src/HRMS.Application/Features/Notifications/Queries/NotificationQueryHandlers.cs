using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Notifications.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<IReadOnlyList<NotificationDto>>>;
public record GetNotificationCountQuery : IRequest<Result<int>>;
public record GetNotificationPreferencesQuery : IRequest<Result<NotificationPreferencesDto>>;

public class NotificationQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper) 
    : IRequestHandler<GetMyNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>,
      IRequestHandler<GetNotificationCountQuery, Result<int>>,
      IRequestHandler<GetNotificationPreferencesQuery, Result<NotificationPreferencesDto>>
{
    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<IReadOnlyList<NotificationDto>>.Failure("Unauthorized.");

        var notifications = await unitOfWork.Notifications.GetUserNotificationsAsync(userId.Value, request.Page, request.PageSize, cancellationToken);
        return Result<IReadOnlyList<NotificationDto>>.Success(mapper.Map<IReadOnlyList<NotificationDto>>(notifications));
    }

    public async Task<Result<int>> Handle(GetNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<int>.Failure("Unauthorized.");

        var count = await unitOfWork.Notifications.GetUnreadCountAsync(userId.Value, cancellationToken);
        return Result<int>.Success(count);
    }

    public async Task<Result<NotificationPreferencesDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<NotificationPreferencesDto>.Failure("Unauthorized.");

        var prefs = await unitOfWork.NotificationPreferences.GetByUserIdAsync(userId.Value, cancellationToken);
        if (prefs == null) return Result<NotificationPreferencesDto>.Success(new NotificationPreferencesDto(true, true, true, true, true, true));

        return Result<NotificationPreferencesDto>.Success(mapper.Map<NotificationPreferencesDto>(prefs));
    }
}
