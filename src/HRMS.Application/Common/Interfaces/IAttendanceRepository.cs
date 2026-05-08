using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Common.Interfaces;

public interface IAttendanceRepository : IGenericRepository<AttendanceRecord>
{
    Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecord>> GetTeamAttendanceAsync(Guid managerId, DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecord>> GetTeamAttendanceRangeAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default);
}
