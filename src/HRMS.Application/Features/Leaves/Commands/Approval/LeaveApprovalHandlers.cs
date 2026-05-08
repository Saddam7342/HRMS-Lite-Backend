using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Domain.Events;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Leaves.Commands.Approval;

public record ApproveLeaveRequestCommand(Guid Id) : IRequest<Result>;
public record RejectLeaveRequestCommand(Guid Id, string? RejectionReason) : IRequest<Result>;

public class LeaveApprovalHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator) 
    : IRequestHandler<ApproveLeaveRequestCommand, Result>,
      IRequestHandler<RejectLeaveRequestCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetWithDetailsAsync(request.Id, cancellationToken);
        if (leaveRequest == null) return Result.Failure("Leave request not found.");

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
            return Result.Failure("Only pending requests can be approved.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        var employee = leaveRequest.Employee;
        if (employee == null) return Result.Failure("Employee not found.");

        bool isOrgAdmin = currentUserService.Roles.Contains("OrganizationAdmin");
        bool isManager = employee.ManagerId == approver.Id;

        if (!isOrgAdmin && !isManager)
            return Result.Failure("You are not authorized to approve this leave request.");

        if (approver.Id == leaveRequest.EmployeeId)
            return Result.Failure("You cannot approve your own leave request.");

        var balance = await unitOfWork.LeaveBalances.GetByEmployeeAndTypeAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, leaveRequest.StartDate.Year, cancellationToken);
        if (balance == null || balance.RemainingDays < leaveRequest.TotalDays)
            return Result.Failure("Insufficient leave balance at time of approval.");

        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.ApprovedById = approver.Id;
        leaveRequest.ApprovedAt = dateTimeProvider.UtcNow;

        balance.UsedDays += leaveRequest.TotalDays;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new LeaveStatusChangedEvent(leaveRequest), cancellationToken);

        return Result.Success("Leave request approved.");
    }

    public async Task<Result> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetWithDetailsAsync(request.Id, cancellationToken);
        if (leaveRequest == null) return Result.Failure("Leave request not found.");

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
            return Result.Failure("Only pending requests can be rejected.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        leaveRequest.Status = LeaveRequestStatus.Rejected;
        leaveRequest.ApprovedById = approver.Id;
        leaveRequest.ApprovedAt = dateTimeProvider.UtcNow;
        leaveRequest.RejectionReason = request.RejectionReason;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new LeaveStatusChangedEvent(leaveRequest, request.RejectionReason), cancellationToken);

        return Result.Success("Leave request rejected.");
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
