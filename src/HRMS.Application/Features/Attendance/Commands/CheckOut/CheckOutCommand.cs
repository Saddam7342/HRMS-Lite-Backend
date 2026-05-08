using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Attendance.Commands.CheckOut;

public record CheckOutCommand(string? Notes) : IRequest<Result>;

public class CheckOutHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CheckOutCommand, Result>
{
    public async Task<Result> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result.Failure("Employee profile not found.");

        var now = dateTimeProvider.UtcNow;
        var today = now.Date;

        var attendance = await unitOfWork.Attendance.GetTodayRecordAsync(employee.Id, today, cancellationToken);
        
        if (attendance == null)
            return Result.Failure("No check-in record found for today. Please check-in first.");

        if (attendance.Status == AttendanceStatus.CheckedOut)
            return Result.Failure("You have already checked out for today.");

        var checkOutTime = now.TimeOfDay;
        attendance.CheckOutTime = checkOutTime;
        attendance.Status = AttendanceStatus.CheckedOut;
        
        if (!string.IsNullOrEmpty(request.Notes))
            attendance.Notes = string.IsNullOrEmpty(attendance.Notes) ? request.Notes : $"{attendance.Notes} | OUT: {request.Notes}";

        // Calculate Total Hours
        if (attendance.CheckInTime.HasValue)
        {
            var diff = checkOutTime - attendance.CheckInTime.Value;
            attendance.TotalHours = (decimal)diff.TotalHours;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success("Checked out successfully.");
    }
}
