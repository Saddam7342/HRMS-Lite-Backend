using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class TravelRequestRepository(AppDbContext context) : GenericRepository<TravelRequest>(context), ITravelRequestRepository
{
    public async Task<IReadOnlyList<TravelRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.FromDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TravelRequest>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Where(x => x.Employee.ManagerId == managerId && x.Status == TravelRequestStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TravelRequest>> GetTeamScheduleAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Where(x => (x.Employee.ManagerId == managerId || x.EmployeeId == managerId) && 
                        x.FromDate <= end && x.ToDate >= start &&
                        (x.Status == TravelRequestStatus.Approved || x.Status == TravelRequestStatus.Pending))
            .ToListAsync(ct);
    }

    public async Task<bool> HasOverlappingTravelAsync(Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _dbSet
            .AnyAsync(x => x.EmployeeId == employeeId && 
                           x.Id != excludeId &&
                           x.Status != TravelRequestStatus.Cancelled && 
                           x.Status != TravelRequestStatus.Rejected &&
                           x.FromDate <= end && x.ToDate >= start, ct);
    }

    public async Task<TravelRequest?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Include(x => x.ApprovedBy)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<TravelRequest>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .OrderByDescending(x => x.FromDate)
            .ToListAsync(ct);
    }
}
