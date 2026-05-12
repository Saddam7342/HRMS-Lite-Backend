using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Travel.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Travel.Queries;

public record GetMyTravelRequestsQuery : IRequest<Result<IReadOnlyList<TravelRequestDto>>>;
public record GetPendingTravelApprovalsQuery : IRequest<Result<IReadOnlyList<TravelRequestDto>>>;
public record GetTeamTravelScheduleQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<IReadOnlyList<TeamTravelScheduleDto>>>;
public record GetTravelHistoryQuery : IRequest<Result<IReadOnlyList<TravelRequestListDto>>>;
public record GetTravelRequestByIdQuery(Guid Id) : IRequest<Result<TravelRequestDto>>;

public class TravelQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper) 
    : IRequestHandler<GetMyTravelRequestsQuery, Result<IReadOnlyList<TravelRequestDto>>>,
      IRequestHandler<GetPendingTravelApprovalsQuery, Result<IReadOnlyList<TravelRequestDto>>>,
      IRequestHandler<GetTeamTravelScheduleQuery, Result<IReadOnlyList<TeamTravelScheduleDto>>>,
      IRequestHandler<GetTravelHistoryQuery, Result<IReadOnlyList<TravelRequestListDto>>>,
      IRequestHandler<GetTravelRequestByIdQuery, Result<TravelRequestDto>>
{
    public async Task<Result<IReadOnlyList<TravelRequestDto>>> Handle(GetMyTravelRequestsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<TravelRequestDto>>.Failure("Employee not found.");

        var travels = await unitOfWork.TravelRequests.GetByEmployeeAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<TravelRequestDto>>.Success(mapper.Map<List<TravelRequestDto>>(travels));
    }

    public async Task<Result<IReadOnlyList<TravelRequestDto>>> Handle(GetPendingTravelApprovalsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<TravelRequestDto>>.Failure("Employee not found.");

        var pending = await unitOfWork.TravelRequests.GetPendingByManagerAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<TravelRequestDto>>.Success(mapper.Map<List<TravelRequestDto>>(pending));
    }

    public async Task<Result<IReadOnlyList<TeamTravelScheduleDto>>> Handle(GetTeamTravelScheduleQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<TeamTravelScheduleDto>>.Failure("Employee not found.");

        var teamSchedule = await unitOfWork.TravelRequests.GetTeamScheduleAsync(employee.Id, request.StartDate, request.EndDate, cancellationToken);
        return Result<IReadOnlyList<TeamTravelScheduleDto>>.Success(mapper.Map<List<TeamTravelScheduleDto>>(teamSchedule));
    }

    public async Task<Result<IReadOnlyList<TravelRequestListDto>>> Handle(GetTravelHistoryQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<TravelRequestListDto>>.Failure("Employee not found.");

        var travels = await unitOfWork.TravelRequests.GetByEmployeeAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<TravelRequestListDto>>.Success(mapper.Map<List<TravelRequestListDto>>(travels));
    }

    public async Task<Result<TravelRequestDto>> Handle(GetTravelRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var travel = await unitOfWork.TravelRequests.GetWithDetailsAsync(request.Id, cancellationToken);
        if (travel == null) return Result<TravelRequestDto>.Failure("Travel request not found.");

        return Result<TravelRequestDto>.Success(mapper.Map<TravelRequestDto>(travel));
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
