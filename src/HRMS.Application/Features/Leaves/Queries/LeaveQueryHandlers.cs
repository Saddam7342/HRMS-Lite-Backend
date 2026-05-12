using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Leaves.DTOs;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Leaves.Queries;

public record GetMyLeaveRequestsQuery : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;
public record GetPendingLeaveApprovalsQuery : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;
public record GetTeamLeaveCalendarQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<IReadOnlyList<LeaveCalendarDto>>>;
public record GetLeaveBalancesQuery(int? Year) : IRequest<Result<IReadOnlyList<LeaveBalanceDto>>>;

public class LeaveQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) 
    : IRequestHandler<GetMyLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestDto>>>,
      IRequestHandler<GetPendingLeaveApprovalsQuery, Result<IReadOnlyList<LeaveRequestDto>>>,
      IRequestHandler<GetTeamLeaveCalendarQuery, Result<IReadOnlyList<LeaveCalendarDto>>>,
      IRequestHandler<GetLeaveBalancesQuery, Result<IReadOnlyList<LeaveBalanceDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetMyLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<LeaveRequestDto>>.Failure("Employee not found.");

        var leaves = await unitOfWork.LeaveRequests.GetByEmployeeAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(mapper.Map<List<LeaveRequestDto>>(leaves));
    }

    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetPendingLeaveApprovalsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<LeaveRequestDto>>.Failure("Employee not found.");

        var pending = await unitOfWork.LeaveRequests.GetPendingByManagerAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(mapper.Map<List<LeaveRequestDto>>(pending));
    }

    public async Task<Result<IReadOnlyList<LeaveCalendarDto>>> Handle(GetTeamLeaveCalendarQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<LeaveCalendarDto>>.Failure("Employee not found.");

        var teamLeaves = await unitOfWork.LeaveRequests.GetTeamLeaveAsync(employee.Id, request.StartDate, request.EndDate, cancellationToken);
        return Result<IReadOnlyList<LeaveCalendarDto>>.Success(mapper.Map<List<LeaveCalendarDto>>(teamLeaves));
    }

    public async Task<Result<IReadOnlyList<LeaveBalanceDto>>> Handle(GetLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<LeaveBalanceDto>>.Failure("Employee not found.");

        var year = request.Year ?? dateTimeProvider.UtcNow.Year;
        var balances = await unitOfWork.LeaveBalances.GetByEmployeeAsync(employee.Id, year, cancellationToken);
        
        return Result<IReadOnlyList<LeaveBalanceDto>>.Success(mapper.Map<List<LeaveBalanceDto>>(balances));
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
