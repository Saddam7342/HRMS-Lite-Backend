using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Attendance.Commands.Update;

public record UpdateAttendanceCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public TimeSpan? CheckInTime { get; init; }
    public TimeSpan? CheckOutTime { get; init; }
    public AttendanceStatus Status { get; init; }
    public string? Notes { get; init; }
}

public record MarkAttendanceAbsentCommand(Guid EmployeeId, DateTime Date) : IRequest<Result>;

public class AttendanceUpdateHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) 
    : IRequestHandler<UpdateAttendanceCommand, Result>,
      IRequestHandler<MarkAttendanceAbsentCommand, Result>
{
    public async Task<Result> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await unitOfWork.Attendance.GetByIdAsync(request.Id, cancellationToken);
        if (attendance == null) return Result.Failure("Attendance record not found.");

        // Security: Admin only for override
        if (!currentUserService.Roles.Contains("Admin"))
            return Result.Failure("Unauthorized. Only administrators can override attendance records.");

        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.Status = request.Status;
        attendance.Notes = $"{attendance.Notes} | Admin Override: {request.Notes}";

        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            var diff = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
            attendance.TotalHours = (decimal)diff.TotalHours;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success("Attendance record updated by administrator.");
    }

    public async Task<Result> Handle(MarkAttendanceAbsentCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Attendance.GetTodayRecordAsync(request.EmployeeId, request.Date, cancellationToken);
        if (existing != null) return Result.Failure("An attendance record already exists for this date.");

        var employee = await unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null) return Result.Failure("Employee not found.");

        var attendance = new Domain.Entities.AttendanceRecord
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date.Date,
            Status = AttendanceStatus.Absent,
            Notes = "Marked absent by administrator."
        };

        await unitOfWork.Attendance.AddAsync(attendance, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Employee marked as absent.");
    }
}
