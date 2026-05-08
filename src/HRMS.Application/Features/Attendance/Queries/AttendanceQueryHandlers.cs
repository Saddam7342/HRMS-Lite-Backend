using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Attendance.DTOs;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Attendance.Queries;

public record GetMyAttendanceQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<IReadOnlyList<AttendanceDto>>>;
public record GetTodayAttendanceQuery : IRequest<Result<AttendanceDto>>;
public record GetAttendanceByDateRangeQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<IReadOnlyList<AttendanceDto>>>;
public record GetTeamAttendanceQuery(DateTime Date) : IRequest<Result<IReadOnlyList<AttendanceListDto>>>;
public record GetAttendanceSummaryQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<AttendanceSummaryDto>>;

public class AttendanceQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) 
    : IRequestHandler<GetMyAttendanceQuery, Result<IReadOnlyList<AttendanceDto>>>,
      IRequestHandler<GetTodayAttendanceQuery, Result<AttendanceDto>>,
      IRequestHandler<GetAttendanceByDateRangeQuery, Result<IReadOnlyList<AttendanceDto>>>,
      IRequestHandler<GetTeamAttendanceQuery, Result<IReadOnlyList<AttendanceListDto>>>,
      IRequestHandler<GetAttendanceSummaryQuery, Result<AttendanceSummaryDto>>
{
    public async Task<Result<IReadOnlyList<AttendanceDto>>> Handle(GetMyAttendanceQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<AttendanceDto>>.Failure("Employee not found.");

        var records = await unitOfWork.Attendance.GetByEmployeeAsync(employee.Id, request.StartDate, request.EndDate, cancellationToken);
        return Result<IReadOnlyList<AttendanceDto>>.Success(mapper.Map<IReadOnlyList<AttendanceDto>>(records));
    }

    public async Task<Result<AttendanceDto>> Handle(GetTodayAttendanceQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<AttendanceDto>.Failure("Employee not found.");

        var record = await unitOfWork.Attendance.GetTodayRecordAsync(employee.Id, dateTimeProvider.UtcNow.Date, cancellationToken);
        if (record == null) return Result<AttendanceDto>.Failure("No attendance record for today.");

        return Result<AttendanceDto>.Success(mapper.Map<AttendanceDto>(record));
    }

    public async Task<Result<IReadOnlyList<AttendanceDto>>> Handle(GetAttendanceByDateRangeQuery request, CancellationToken cancellationToken)
    {
        // General query for admins
        var records = await unitOfWork.Attendance.GetByEmployeeAsync(Guid.Empty, request.StartDate, request.EndDate, cancellationToken);
        return Result<IReadOnlyList<AttendanceDto>>.Success(mapper.Map<IReadOnlyList<AttendanceDto>>(records));
    }

    public async Task<Result<IReadOnlyList<AttendanceListDto>>> Handle(GetTeamAttendanceQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<AttendanceListDto>>.Failure("Employee not found.");

        var team = await unitOfWork.Attendance.GetTeamAttendanceAsync(employee.Id, request.Date, cancellationToken);
        return Result<IReadOnlyList<AttendanceListDto>>.Success(mapper.Map<IReadOnlyList<AttendanceListDto>>(team));
    }

    public async Task<Result<AttendanceSummaryDto>> Handle(GetAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<AttendanceSummaryDto>.Failure("Employee not found.");

        var records = await unitOfWork.Attendance.GetByEmployeeAsync(employee.Id, request.StartDate, request.EndDate, cancellationToken);
        
        var summary = new AttendanceSummaryDto(
            PresentDays: records.Count(x => x.Status == AttendanceStatus.CheckedOut || x.Status == AttendanceStatus.CheckedIn),
            LateDays: records.Count(x => x.IsLate),
            AbsentDays: records.Count(x => x.Status == AttendanceStatus.Absent),
            TotalHoursWorked: records.Sum(x => x.TotalHours ?? 0)
        );

        return Result<AttendanceSummaryDto>.Success(summary);
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
