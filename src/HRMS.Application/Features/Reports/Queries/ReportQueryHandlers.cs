using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Reports.DTOs;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Reports.Queries;

public record GetLeaveAnalyticsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<Result<LeaveSummaryDto>>;
public record GetExpenseAnalyticsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<Result<ExpenseSummaryDto>>;
public record GetAttendanceAnalyticsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<Result<AttendanceSummaryDto>>;

public class ReportQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) 
    : IRequestHandler<GetLeaveAnalyticsQuery, Result<LeaveSummaryDto>>,
      IRequestHandler<GetExpenseAnalyticsQuery, Result<ExpenseSummaryDto>>,
      IRequestHandler<GetAttendanceAnalyticsQuery, Result<AttendanceSummaryDto>>
{
    public async Task<Result<LeaveSummaryDto>> Handle(GetLeaveAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.DbContext.LeaveRequests.AsNoTracking();

        if (currentUserService.Roles.Contains("Manager") && !OrgRoles.IsCompanyAdmin(currentUserService.Roles))
        {
            if (!currentUserService.UserId.HasValue)
                return Result<LeaveSummaryDto>.Success(new LeaveSummaryDto(0, 0, 0, 0, []));
            var mgr = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, cancellationToken);
            if (mgr == null)
                return Result<LeaveSummaryDto>.Success(new LeaveSummaryDto(0, 0, 0, 0, []));
            query = query.Where(x => x.Employee.ManagerId == mgr.Id);
        }

        if (request.StartDate.HasValue) query = query.Where(x => x.StartDate >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.EndDate <= request.EndDate.Value);

        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(x => x.Status == LeaveRequestStatus.Pending, cancellationToken);
        var approved = await query.CountAsync(x => x.Status == LeaveRequestStatus.Approved, cancellationToken);
        var rejected = await query.CountAsync(x => x.Status == LeaveRequestStatus.Rejected, cancellationToken);

        var dist = await query
            .GroupBy(x => x.LeaveType.Name)
            .Select(g => new LeaveTypeDistributionDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return Result<LeaveSummaryDto>.Success(new LeaveSummaryDto(total, pending, approved, rejected, dist));
    }

    public async Task<Result<ExpenseSummaryDto>> Handle(GetExpenseAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.DbContext.ExpenseClaims.AsNoTracking();

        if (currentUserService.Roles.Contains("Manager") && !OrgRoles.IsCompanyAdmin(currentUserService.Roles))
        {
            if (!currentUserService.UserId.HasValue)
                return Result<ExpenseSummaryDto>.Success(new ExpenseSummaryDto(0, 0, 0, []));
            var mgr = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, cancellationToken);
            if (mgr == null)
                return Result<ExpenseSummaryDto>.Success(new ExpenseSummaryDto(0, 0, 0, []));
            query = query.Where(x => x.Employee.ManagerId == mgr.Id);
        }

        if (request.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

        var totalClaimed = await query.SumAsync(x => x.Amount, cancellationToken);
        var approved = await query.Where(x => x.Status == ExpenseClaimStatus.Approved).SumAsync(x => x.Amount, cancellationToken);
        var pending = await query.Where(x => x.Status == ExpenseClaimStatus.Pending).SumAsync(x => x.Amount, cancellationToken);

        var dist = await query
            .GroupBy(x => x.Category.Name)
            .Select(g => new ExpenseCategorySpendingDto(g.Key, g.Sum(x => x.Amount)))
            .ToListAsync(cancellationToken);

        return Result<ExpenseSummaryDto>.Success(new ExpenseSummaryDto(totalClaimed, approved, pending, dist));
    }

    public async Task<Result<AttendanceSummaryDto>> Handle(GetAttendanceAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.DbContext.AttendanceRecords.AsNoTracking();

        if (currentUserService.Roles.Contains("Manager") && !OrgRoles.IsCompanyAdmin(currentUserService.Roles))
        {
            if (!currentUserService.UserId.HasValue)
                return Result<AttendanceSummaryDto>.Success(new AttendanceSummaryDto(0, 0, 0, 0));
            var mgr = await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, cancellationToken);
            if (mgr == null)
                return Result<AttendanceSummaryDto>.Success(new AttendanceSummaryDto(0, 0, 0, 0));
            query = query.Where(x => x.Employee.ManagerId == mgr.Id);
        }

        if (request.StartDate.HasValue) query = query.Where(x => x.Date >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.Date <= request.EndDate.Value);

        var avgHours = await query.Where(x => x.TotalHours > 0).AverageAsync(x => (double?)x.TotalHours, cancellationToken) ?? 0;
        var lateCount = await query.CountAsync(x => x.IsLate, cancellationToken);
        var missingCheckout = await query.CountAsync(x => x.CheckOutTime == null, cancellationToken);

        return Result<AttendanceSummaryDto>.Success(new AttendanceSummaryDto(avgHours, 0, lateCount, missingCheckout));
    }
}
