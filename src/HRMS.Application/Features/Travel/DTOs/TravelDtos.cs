using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Travel.DTOs;

public record TravelRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Destination,
    string Purpose,
    DateTime FromDate,
    DateTime ToDate,
    TravelRequestStatus Status,
    decimal? EstimatedBudget,
    string? ApproverName,
    DateTime? ApprovedAt,
    string? RejectionReason);

public record TravelRequestListDto(
    Guid Id,
    string EmployeeName,
    string Destination,
    DateTime FromDate,
    DateTime ToDate,
    TravelRequestStatus Status);

public record TeamTravelScheduleDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Destination,
    DateTime FromDate,
    DateTime ToDate,
    TravelRequestStatus Status);

public record CreateTravelRequestRequest(
    string Destination,
    string Purpose,
    DateTime FromDate,
    DateTime ToDate,
    decimal? EstimatedBudget);

public record UpdateTravelRequestRequest(
    string Destination,
    string Purpose,
    DateTime FromDate,
    DateTime ToDate,
    decimal? EstimatedBudget);
