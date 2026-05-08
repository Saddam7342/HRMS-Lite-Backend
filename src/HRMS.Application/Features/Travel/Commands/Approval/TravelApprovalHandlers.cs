using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Domain.Events;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Travel.Commands.Approval;

public record ApproveTravelRequestCommand(Guid Id) : IRequest<Result>;
public record RejectTravelRequestCommand(Guid Id, string? RejectionReason) : IRequest<Result>;

public class TravelApprovalHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator) 
    : IRequestHandler<ApproveTravelRequestCommand, Result>,
      IRequestHandler<RejectTravelRequestCommand, Result>
{
    public async Task<Result> Handle(ApproveTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travel = await unitOfWork.TravelRequests.GetWithDetailsAsync(request.Id, cancellationToken);
        if (travel == null) return Result.Failure("Travel request not found.");

        if (travel.Status != TravelRequestStatus.Pending)
            return Result.Failure("Only pending requests can be approved.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        // Security: Approver must be Manager or OrgAdmin
        bool isOrgAdmin = currentUserService.Roles.Contains("OrganizationAdmin");
        bool isManager = travel.Employee.ManagerId == approver.Id;

        if (!isOrgAdmin && !isManager)
            return Result.Failure("You are not authorized to approve this travel request.");

        if (approver.Id == travel.EmployeeId)
            return Result.Failure("You cannot approve your own travel request.");

        travel.Status = TravelRequestStatus.Approved;
        travel.ApprovedById = approver.Id;
        travel.ApprovedAt = dateTimeProvider.UtcNow;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new TravelStatusChangedEvent(travel), cancellationToken);

        return Result.Success("Travel request approved.");
    }

    public async Task<Result> Handle(RejectTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travel = await unitOfWork.TravelRequests.GetWithDetailsAsync(request.Id, cancellationToken);
        if (travel == null) return Result.Failure("Travel request not found.");

        if (travel.Status != TravelRequestStatus.Pending)
            return Result.Failure("Only pending requests can be rejected.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        travel.Status = TravelRequestStatus.Rejected;
        travel.ApprovedById = approver.Id;
        travel.ApprovedAt = dateTimeProvider.UtcNow;
        travel.RejectionReason = request.RejectionReason;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new TravelStatusChangedEvent(travel, request.RejectionReason), cancellationToken);

        return Result.Success("Travel request rejected.");
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
