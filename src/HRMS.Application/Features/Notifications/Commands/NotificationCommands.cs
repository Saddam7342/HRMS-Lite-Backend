using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Notifications.Commands;

public record CreateNotificationCommand(
    Guid UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? RelatedEntityId = null,
    string? RelatedEntityType = null) : IRequest<Result<Guid>>;

public record MarkAsReadCommand(Guid Id) : IRequest<Result>;
public record MarkAllAsReadCommand : IRequest<Result>;
public record DeleteNotificationCommand(Guid Id) : IRequest<Result>;

public class NotificationCommandHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) 
    : IRequestHandler<CreateNotificationCommand, Result<Guid>>,
      IRequestHandler<MarkAsReadCommand, Result>,
      IRequestHandler<MarkAllAsReadCommand, Result>,
      IRequestHandler<DeleteNotificationCommand, Result>
{
    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return Result<Guid>.Failure("User not found.");

        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType
        };

        await unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(notification.Id);
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(request.Id, cancellationToken);
        if (notification == null) return Result.Failure("Notification not found.");

        if (notification.UserId != currentUserService.UserId)
            return Result.Failure("Unauthorized.");

        notification.IsRead = true;
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result.Failure("Unauthorized.");

        await unitOfWork.Notifications.MarkAllAsReadAsync(userId.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(request.Id, cancellationToken);
        if (notification == null) return Result.Failure("Notification not found.");

        if (notification.UserId != currentUserService.UserId)
            return Result.Failure("Unauthorized.");

        unitOfWork.Notifications.Remove(notification);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
