using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class LeaveTypeRepository(AppDbContext context) : GenericRepository<LeaveType>(context), ILeaveTypeRepository
{
    public async Task<IReadOnlyList<LeaveType>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _dbSet.Where(x => x.IsActive).ToListAsync(ct);
    }
}

public class LeaveBalanceRepository(AppDbContext context) : GenericRepository<LeaveBalance>(context), ILeaveBalanceRepository
{
    public async Task<IReadOnlyList<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, int year, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId && x.Year == year)
            .ToListAsync(ct);
    }

    public async Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, Guid leaveTypeId, int year, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveTypeId && x.Year == year, ct);
    }
}

public class LeaveRequestRepository(AppDbContext context) : GenericRepository<LeaveRequest>(context), ILeaveRequestRepository
{
    public async Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => x.Employee.ManagerId == managerId && x.Status == LeaveRequestStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => x.Status == status)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetUsedDaysAsync(Guid employeeId, Guid leaveTypeId, int year, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.EmployeeId == employeeId &&
                        x.LeaveTypeId == leaveTypeId &&
                        x.Status == LeaveRequestStatus.Approved &&
                        x.StartDate.Year == year)
            .SumAsync(x => x.TotalDays, ct);
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetTeamLeaveAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => (x.Employee.ManagerId == managerId || x.EmployeeId == managerId) &&
                        x.StartDate <= end && x.EndDate >= start &&
                        (x.Status == LeaveRequestStatus.Approved || x.Status == LeaveRequestStatus.Pending))
            .ToListAsync(ct);
    }

    public async Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _dbSet
            .AnyAsync(x => x.EmployeeId == employeeId &&
                           x.Id != excludeId &&
                           x.Status != LeaveRequestStatus.Cancelled &&
                           x.Status != LeaveRequestStatus.Rejected &&
                           x.StartDate <= end && x.EndDate >= start, ct);
    }

    public async Task<LeaveRequest?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedBy)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
