using HRMS.Application.Common;
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
        var isCompanyAdmin = OrgRoles.IsCompanyAdmin(currentUserService.Roles);
        var isManagerScoped = currentUserService.Roles.Contains("Manager") && !isCompanyAdmin;
        Guid? managerEmployeeId = null;
        if (isManagerScoped && currentUserService.UserId.HasValue)
        {
            var me = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, cancellationToken);
            managerEmployeeId = me?.Id;
        }

        var startOfMonth = new DateTime(dateTimeProvider.UtcNow.Year, dateTimeProvider.UtcNow.Month, 1);

        // 1. Employee Stats
        var empQuery = unitOfWork.DbContext.Employees.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            empQuery = empQuery.Where(x => x.ManagerId == managerEmployeeId || x.Id == managerEmployeeId);

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
        if (isManagerScoped && managerEmployeeId.HasValue)
            leaveQuery = leaveQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

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
        if (isManagerScoped && managerEmployeeId.HasValue)
            expenseQuery = expenseQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

        var totalClaimed = await expenseQuery.SumAsync(x => x.Amount, cancellationToken);
        var approvedExpenses = await expenseQuery.Where(x => x.Status == ExpenseClaimStatus.Approved).SumAsync(x => x.Amount, cancellationToken);
        var pendingExpenses = await expenseQuery.Where(x => x.Status == ExpenseClaimStatus.Pending).SumAsync(x => x.Amount, cancellationToken);

        var catSpending = await expenseQuery
            .GroupBy(x => x.Category.Name)
            .Select(g => new ExpenseCategorySpendingDto(g.Key, g.Sum(x => x.Amount)))
            .ToListAsync(cancellationToken);

        // 4. Travel Stats
        var travelQuery = unitOfWork.DbContext.TravelRequests.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            travelQuery = travelQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

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
        if (isManagerScoped && managerEmployeeId.HasValue)
            attendanceQuery = attendanceQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

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
