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
        var today = dateTimeProvider.UtcNow.Date;

        // 1. Employee stats — 2 round-trips (aggregates + department distribution) instead of 4+
        var empQuery = unitOfWork.DbContext.Employees.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            empQuery = empQuery.Where(x => x.ManagerId == managerEmployeeId || x.Id == managerEmployeeId);

        var empAgg = await empQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(e => e.Status == EmployeeStatus.Active),
                NewHires = g.Count(e => e.HireDate >= startOfMonth)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalEmployees = empAgg?.Total ?? 0;
        var activeEmployees = empAgg?.Active ?? 0;
        var newHires = empAgg?.NewHires ?? 0;

        var deptDistribution = await empQuery
            .Where(x => x.DepartmentId != null && x.Department != null)
            .GroupBy(x => x.Department!.Name)
            .Select(g => new DepartmentDistributionDto(g.Key ?? "Unknown", g.Count()))
            .ToListAsync(cancellationToken);

        // 2. Leave stats — 2 round-trips instead of 5
        var leaveQuery = unitOfWork.DbContext.LeaveRequests.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            leaveQuery = leaveQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

        var leaveAgg = await leaveQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(x => x.Status == LeaveRequestStatus.Pending),
                Approved = g.Count(x => x.Status == LeaveRequestStatus.Approved),
                Rejected = g.Count(x => x.Status == LeaveRequestStatus.Rejected)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalLeaves = leaveAgg?.Total ?? 0;
        var pendingLeaves = leaveAgg?.Pending ?? 0;
        var approvedLeaves = leaveAgg?.Approved ?? 0;
        var rejectedLeaves = leaveAgg?.Rejected ?? 0;

        var leaveTypeDist = await leaveQuery
            .Where(x => x.LeaveType != null)
            .GroupBy(x => x.LeaveType.Name)
            .Select(g => new LeaveTypeDistributionDto(g.Key ?? "Unknown", g.Count()))
            .ToListAsync(cancellationToken);

        // 3. Expense stats — 2 round-trips instead of 4
        var expenseQuery = unitOfWork.DbContext.ExpenseClaims.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            expenseQuery = expenseQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

        var expenseAgg = await expenseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalClaimed = g.Sum(x => x.Amount),
                Approved = g.Where(x => x.Status == ExpenseClaimStatus.Approved).Sum(x => x.Amount),
                Pending = g.Where(x => x.Status == ExpenseClaimStatus.Pending).Sum(x => x.Amount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalClaimed = expenseAgg?.TotalClaimed ?? 0;
        var approvedExpenses = expenseAgg?.Approved ?? 0;
        var pendingExpenses = expenseAgg?.Pending ?? 0;

        var catSpending = await expenseQuery
            .Where(x => x.Category != null)
            .GroupBy(x => x.Category.Name)
            .Select(g => new ExpenseCategorySpendingDto(g.Key ?? "Unknown", g.Sum(x => x.Amount)))
            .ToListAsync(cancellationToken);


        // 4. Travel stats — 2 round-trips instead of 4
        var travelQuery = unitOfWork.DbContext.TravelRequests.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            travelQuery = travelQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

        var travelAgg = await travelQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Approved = g.Count(x => x.Status == TravelRequestStatus.Approved),
                Pending = g.Count(x => x.Status == TravelRequestStatus.Pending)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalTravel = travelAgg?.Total ?? 0;
        var approvedTravel = travelAgg?.Approved ?? 0;
        var pendingTravel = travelAgg?.Pending ?? 0;

        var destDist = await travelQuery
            .GroupBy(x => x.Destination)
            .Select(g => new TravelDestinationDistributionDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Attendance stats — 2 round-trips instead of 3 (average is separate: simpler SQL than one grouped mega-query)
        var attendanceQuery = unitOfWork.DbContext.AttendanceRecords.AsNoTracking();
        if (isManagerScoped && managerEmployeeId.HasValue)
            attendanceQuery = attendanceQuery.Where(x => x.Employee.ManagerId == managerEmployeeId);

        var avgHours = await attendanceQuery.Where(x => x.TotalHours > 0)
            .AverageAsync(x => (double?)x.TotalHours, cancellationToken) ?? 0;

        var attendanceAgg = await attendanceQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                LateCount = g.Count(x => x.IsLate),
                MissingCheckout = g.Count(x => x.CheckOutTime == null && x.Date < today)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var lateCount = attendanceAgg?.LateCount ?? 0;
        var missingCheckout = attendanceAgg?.MissingCheckout ?? 0;

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
