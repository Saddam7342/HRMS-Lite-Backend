using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Domain.Events;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Expenses.Commands.Approval;

public record ApproveExpenseClaimCommand(Guid Id) : IRequest<Result>;
public record RejectExpenseClaimCommand(Guid Id, string? RejectionReason) : IRequest<Result>;

public class ExpenseApprovalHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator) 
    : IRequestHandler<ApproveExpenseClaimCommand, Result>,
      IRequestHandler<RejectExpenseClaimCommand, Result>
{
    public async Task<Result> Handle(ApproveExpenseClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await unitOfWork.ExpenseClaims.GetWithDetailsAsync(request.Id, cancellationToken);
        if (claim == null) return Result.Failure("Expense claim not found.");

        if (claim.Status != ExpenseClaimStatus.Pending)
            return Result.Failure("Only pending claims can be approved.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        bool isOrgAdmin = currentUserService.Roles.Contains("OrganizationAdmin");
        bool isManager = claim.Employee.ManagerId == approver.Id;

        if (!isOrgAdmin && !isManager)
            return Result.Failure("You are not authorized to approve this claim.");

        if (approver.Id == claim.EmployeeId)
            return Result.Failure("You cannot approve your own expense claim.");

        claim.Status = ExpenseClaimStatus.Approved;
        claim.ApprovedById = approver.Id;
        claim.ApprovedAt = dateTimeProvider.UtcNow;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new ExpenseStatusChangedEvent(claim), cancellationToken);

        return Result.Success("Expense claim approved.");
    }

    public async Task<Result> Handle(RejectExpenseClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await unitOfWork.ExpenseClaims.GetWithDetailsAsync(request.Id, cancellationToken);
        if (claim == null) return Result.Failure("Expense claim not found.");

        if (claim.Status != ExpenseClaimStatus.Pending)
            return Result.Failure("Only pending claims can be rejected.");

        var approver = await GetCurrentEmployeeAsync(cancellationToken);
        if (approver == null) return Result.Failure("Approver profile not found.");

        claim.Status = ExpenseClaimStatus.Rejected;
        claim.ApprovedById = approver.Id;
        claim.ApprovedAt = dateTimeProvider.UtcNow;
        claim.RejectionReason = request.RejectionReason;

        await unitOfWork.CommitAsync(cancellationToken);

        // Publish Event
        await mediator.Publish(new ExpenseStatusChangedEvent(claim, request.RejectionReason), cancellationToken);

        return Result.Success("Expense claim rejected.");
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
