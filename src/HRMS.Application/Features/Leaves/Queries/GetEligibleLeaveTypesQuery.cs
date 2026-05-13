using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Leaves.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Leaves.Queries;

public record GetEligibleLeaveTypesQuery : IRequest<Result<IReadOnlyList<LeaveTypeOptionDto>>>;

public class GetEligibleLeaveTypesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetEligibleLeaveTypesQuery, Result<IReadOnlyList<LeaveTypeOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveTypeOptionDto>>> Handle(GetEligibleLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Result<IReadOnlyList<LeaveTypeOptionDto>>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (employee == null)
            return Result<IReadOnlyList<LeaveTypeOptionDto>>.Failure("Employee profile not found.");

        var leaveTypes = await unitOfWork.LeaveTypes.GetAllActiveAsync(cancellationToken);
        var options = leaveTypes
            .Where(lt => !lt.IsGenderSpecific || lt.ApplicableGender == employee.Gender)
            .OrderBy(lt => lt.Name)
            .Select(lt => new LeaveTypeOptionDto(
                lt.Id,
                lt.Name,
                lt.Code,
                lt.DefaultDays,
                lt.IsGenderSpecific,
                lt.ApplicableGender))
            .ToList();

        return Result<IReadOnlyList<LeaveTypeOptionDto>>.Success(options);
    }
}
