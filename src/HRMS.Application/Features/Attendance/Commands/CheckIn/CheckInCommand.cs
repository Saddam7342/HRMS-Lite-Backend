using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Attendance.Commands.CheckIn;

public record CheckInCommand(string? Notes) : IRequest<Result<Guid>>;

public class CheckInHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CheckInCommand, Result<Guid>>
{
    private static readonly TimeSpan StandardStartTime = new(9, 30, 0); // 09:30 AM

    public async Task<Result<Guid>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<Guid>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result<Guid>.Failure("Employee profile not found.");

        var now = dateTimeProvider.UtcNow;
        var today = now.Date;

        // Check for existing record
        var existing = await unitOfWork.Attendance.GetTodayRecordAsync(employee.Id, today, cancellationToken);
        if (existing != null)
            return Result<Guid>.Failure("You have already checked in for today.");

        var checkInTime = now.TimeOfDay;
        var isLate = checkInTime > StandardStartTime;

        var attendance = new AttendanceRecord
        {
            EmployeeId = employee.Id,
            Date = today,
            CheckInTime = checkInTime,
            Status = AttendanceStatus.CheckedIn,
            IsLate = isLate,
            Notes = request.Notes
        };

        await unitOfWork.Attendance.AddAsync(attendance, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(attendance.Id);
    }
}
