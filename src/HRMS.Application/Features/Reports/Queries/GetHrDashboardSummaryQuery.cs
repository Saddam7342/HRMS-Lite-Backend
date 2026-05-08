using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Reports.DTOs;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Reports.Queries;

public record GetHrDashboardSummaryQuery : IRequest<Result<HrDashboardDto>>;

public class GetHrDashboardSummaryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<GetHrDashboardSummaryQuery, Result<HrDashboardDto>>
{
    public async Task<Result<HrDashboardDto>> Handle(GetHrDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var isManager = currentUserService.Roles.Contains("Manager") && !currentUserService.Roles.Contains("OrganizationAdmin");
        var managerId = currentUserService.UserId;
        var startOfMonth = new DateTime(dateTimeProvider.UtcNow.Year, dateTimeProvider.UtcNow.Month, 1);

        // 1. Employee Stats
        var empQuery = unitOfWork.DbContext.Employees.AsNoTracking();
        if (isManager) empQuery = empQuery.Where(x => x.ManagerId == managerId || x.Id == managerId);

        var totalEmployees = await empQuery.CountAsync(cancellationToken);
        var activeEmployees = await empQuery.CountAsync(x => x.Status == EmployeeStatus.Active, cancellationToken);
        var newHires = await empQuery.CountAsync(x => x.HireDate >= startOfMonth, cancellationToken);
        
        var deptDistribution = await empQuery
            .Where(x => x.DepartmentId != null)
            .GroupBy(x => x.Department!.Name)
            .Select(g => new DepartmentDistributionDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        // 2. Leave Stats
        var leaveQuery = unitOfWork.DbContext.LeaveRequests.AsNoTracking();
        if (isManager) leaveQuery = leaveQuery.Where(x => x.Employee.ManagerId == managerId);

        var totalLeaves = await leaveQuery.CountAsync(cancellationToken);
        var pendingLeaves = await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Pending, cancellationToken);
        var approvedLeaves = await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Approved, cancellationToken);
        var rejectedLeaves = await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Rejected, cancellationToken);

        var leaveTypeDist = await leaveQuery
            .GroupBy(x => x.LeaveType.Name)
            .Select(g => new LeaveTypeDistributionDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        // 3. Expense Stats
        var expenseQuery = unitOfWork.DbContext.ExpenseClaims.AsNoTracking();
        if (isManager) expenseQuery = expenseQuery.Where(x => x.Employee.ManagerId == managerId);

        var totalClaimed = await expenseQuery.SumAsync(x => x.Amount, cancellationToken);
        var approvedExpenses = await expenseQuery.Where(x => x.Status == ExpenseClaimStatus.Approved).SumAsync(x => x.Amount, cancellationToken);
        var pendingExpenses = await expenseQuery.Where(x => x.Status == ExpenseClaimStatus.Pending).SumAsync(x => x.Amount, cancellationToken);

        var catSpending = await expenseQuery
            .GroupBy(x => x.Category.Name)
            .Select(g => new ExpenseCategorySpendingDto(g.Key, g.Sum(x => x.Amount)))
            .ToListAsync(cancellationToken);

        // 4. Travel Stats
        var travelQuery = unitOfWork.DbContext.TravelRequests.AsNoTracking();
        if (isManager) travelQuery = travelQuery.Where(x => x.Employee.ManagerId == managerId);

        var totalTravel = await travelQuery.CountAsync(cancellationToken);
        var approvedTravel = await travelQuery.CountAsync(x => x.Status == TravelRequestStatus.Approved, cancellationToken);
        var pendingTravel = await travelQuery.CountAsync(x => x.Status == TravelRequestStatus.Pending, cancellationToken);

        var destDist = await travelQuery
            .GroupBy(x => x.Destination)
            .Select(g => new TravelDestinationDistributionDto(g.Key, g.Count()))
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Attendance Stats
        var attendanceQuery = unitOfWork.DbContext.AttendanceRecords.AsNoTracking();
        if (isManager) attendanceQuery = attendanceQuery.Where(x => x.Employee.ManagerId == managerId);

        var avgHours = await attendanceQuery.Where(x => x.TotalHours > 0).AverageAsync(x => (double?)x.TotalHours, cancellationToken) ?? 0;
        var lateCount = await attendanceQuery.CountAsync(x => x.IsLate, cancellationToken);
        var missingCheckout = await attendanceQuery.CountAsync(x => x.CheckOutTime == null && x.Date < dateTimeProvider.UtcNow.Date, cancellationToken);

        var dashboard = new HrDashboardDto(
            new EmployeeSummaryDto(totalEmployees, activeEmployees, newHires, deptDistribution),
            new LeaveSummaryDto(totalLeaves, pendingLeaves, approvedLeaves, rejectedLeaves, leaveTypeDist),
            new ExpenseSummaryDto(totalClaimed, approvedExpenses, pendingExpenses, catSpending),
            new TravelSummaryDto(totalTravel, approvedTravel, pendingTravel, destDist),
            new AttendanceSummaryDto(avgHours, 0.95, lateCount, missingCheckout) // Ratio hardcoded for demo simplicity
        );

        return Result<HrDashboardDto>.Success(dashboard);
    }
}
