using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Leaves.Commands.Cancel;

public record CancelLeaveRequestCommand(Guid Id) : IRequest<Result>;

public class CancelLeaveRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CancelLeaveRequestCommand, Result>
{
    public async Task<Result> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetByIdAsync(request.Id, cancellationToken);
        if (leaveRequest == null) return Result.Failure("Leave request not found.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId ?? Guid.Empty, cancellationToken);
        if (employee == null || (leaveRequest.EmployeeId != employee.Id && !currentUserService.Roles.Contains("OrganizationAdmin")))
            return Result.Failure("You are not authorized to cancel this request.");

        if (leaveRequest.Status == LeaveRequestStatus.Cancelled || leaveRequest.Status == LeaveRequestStatus.Rejected)
            return Result.Failure("Request is already in a terminal state.");

        // If it was already approved, restore the balance
        if (leaveRequest.Status == LeaveRequestStatus.Approved)
        {
            var balance = await unitOfWork.LeaveBalances.GetByEmployeeAndTypeAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, leaveRequest.StartDate.Year, cancellationToken);
            if (balance != null)
            {
                balance.UsedDays -= leaveRequest.TotalDays;
            }
        }

        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Leave request cancelled.");
    }
}
