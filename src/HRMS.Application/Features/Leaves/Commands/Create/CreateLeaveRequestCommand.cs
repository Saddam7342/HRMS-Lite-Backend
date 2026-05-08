using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Leaves.Commands.Create;

public record CreateLeaveRequestCommand : IRequest<Result<Guid>>
{
    public Guid LeaveTypeId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? Reason { get; init; }
}

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty().LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.EndDate).NotEmpty();
    }
}

public class CreateLeaveRequestHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateLeaveRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<Guid>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result<Guid>.Failure("Employee profile not found.");

        var leaveType = await unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType == null || !leaveType.IsActive)
            return Result<Guid>.Failure("Invalid or inactive leave type.");

        // 1. Gender Eligibility Check
        if (leaveType.IsGenderSpecific && leaveType.ApplicableGender != employee.Gender)
            return Result<Guid>.Failure($"This leave type is only applicable for {leaveType.ApplicableGender} employees.");

        // 2. Overlap Check
        if (await unitOfWork.LeaveRequests.HasOverlappingLeaveAsync(employee.Id, request.StartDate, request.EndDate, null, cancellationToken))
            return Result<Guid>.Failure("You already have an overlapping leave request for these dates.");

        // 3. Balance Check
        var currentYear = dateTimeProvider.UtcNow.Year;
        var balance = await unitOfWork.LeaveBalances.GetByEmployeeAndTypeAsync(employee.Id, request.LeaveTypeId, currentYear, cancellationToken);
        
        var totalDays = (decimal)(request.EndDate - request.StartDate).TotalDays + 1;
        if (balance == null || balance.RemainingDays < totalDays)
            return Result<Guid>.Failure("Insufficient leave balance.");

        // 4. Create Request
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            TenantId = employee.TenantId
        };

        await unitOfWork.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(leaveRequest.Id);
    }
}
