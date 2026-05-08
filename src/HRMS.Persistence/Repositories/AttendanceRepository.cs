using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Persistence.Repositories;

public class AttendanceRepository(AppDbContext context) : GenericRepository<AttendanceRecord>(context), IAttendanceRepository
{
    public async Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, DateTime date, CancellationToken ct = default)
    {
        var targetDate = date.Date;
        return await _dbSet.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == targetDate, ct);
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.EmployeeId == employeeId && x.Date >= start.Date && x.Date <= end.Date)
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetTeamAttendanceAsync(Guid managerId, DateTime date, CancellationToken ct = default)
    {
        var targetDate = date.Date;
        return await _dbSet
            .Include(x => x.Employee)
            .Where(x => x.Employee.ManagerId == managerId && x.Date == targetDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetTeamAttendanceRangeAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Employee)
            .Where(x => (x.Employee.ManagerId == managerId || x.EmployeeId == managerId) && 
                        x.Date >= start.Date && x.Date <= end.Date)
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);
    }
}
