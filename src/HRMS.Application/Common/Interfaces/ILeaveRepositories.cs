using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Common.Interfaces;

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetByStatusAsync(LeaveRequestStatus status, CancellationToken ct = default);
    Task<decimal> GetUsedDaysAsync(Guid employeeId, Guid leaveTypeId, int year, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetTeamLeaveAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default);
    Task<LeaveRequest?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync(CancellationToken ct = default);
}

public interface ILeaveBalanceRepository : IGenericRepository<LeaveBalance>
{
    Task<IReadOnlyList<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, int year, CancellationToken ct = default);
    Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, Guid leaveTypeId, int year, CancellationToken ct = default);
}

public interface ILeaveTypeRepository : IGenericRepository<LeaveType>
{
    Task<IReadOnlyList<LeaveType>> GetAllActiveAsync(CancellationToken ct = default);
}
