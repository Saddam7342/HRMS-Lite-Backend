using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Leaves.DTOs;

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalDays,
    string? Reason,
    LeaveRequestStatus Status,
    string? ApproverName,
    DateTime? ApprovedAt,
    string? RejectionReason);

public record LeaveBalanceDto(
    Guid LeaveTypeId,
    string LeaveTypeName,
    decimal TotalDays,
    decimal UsedDays,
    decimal RemainingDays,
    int Year);

public record LeaveCalendarDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    LeaveRequestStatus Status);

public record CreateLeaveRequestRequest(
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason);

public record LeaveApprovalDto(
    Guid Id,
    bool Approved,
    string? RejectionReason);
