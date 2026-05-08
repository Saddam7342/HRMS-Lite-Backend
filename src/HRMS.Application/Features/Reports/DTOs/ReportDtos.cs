using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Reports.DTOs;

public record HrDashboardDto(
    EmployeeSummaryDto EmployeeSummary,
    LeaveSummaryDto LeaveSummary,
    ExpenseSummaryDto ExpenseSummary,
    TravelSummaryDto TravelSummary,
    AttendanceSummaryDto AttendanceSummary);

public record EmployeeSummaryDto(
    int TotalEmployees,
    int ActiveEmployees,
    int NewHiresThisMonth,
    List<DepartmentDistributionDto> DepartmentDistribution);

public record DepartmentDistributionDto(string DepartmentName, int Count);

public record LeaveSummaryDto(
    int TotalRequests,
    int PendingCount,
    int ApprovedCount,
    int RejectedCount,
    List<LeaveTypeDistributionDto> TypeDistribution);

public record LeaveTypeDistributionDto(string LeaveTypeName, int Count);

public record ExpenseSummaryDto(
    decimal TotalClaimed,
    decimal ApprovedAmount,
    decimal PendingAmount,
    List<ExpenseCategorySpendingDto> CategorySpending);

public record ExpenseCategorySpendingDto(string CategoryName, decimal Amount);

public record TravelSummaryDto(
    int TotalRequests,
    int ApprovedCount,
    int PendingCount,
    List<TravelDestinationDistributionDto> DestinationDistribution);

public record TravelDestinationDistributionDto(string Destination, int Count);

public record AttendanceSummaryDto(
    double AverageWorkingHours,
    double PresenceRatio,
    int LateArrivalsCount,
    int MissingCheckoutsCount);

public record DateRangeFilter(DateTime? StartDate, DateTime? EndDate);
