using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Attendance.DTOs;

public record AttendanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateTime Date,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    decimal? TotalHours,
    AttendanceStatus Status,
    bool IsLate,
    string? Notes);

public record AttendanceListDto(
    Guid Id,
    string EmployeeName,
    DateTime Date,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    AttendanceStatus Status);

public record AttendanceSummaryDto(
    int PresentDays,
    int LateDays,
    int AbsentDays,
    decimal TotalHoursWorked);

public record CheckInRequest(string? Notes);
public record CheckOutRequest(string? Notes);
