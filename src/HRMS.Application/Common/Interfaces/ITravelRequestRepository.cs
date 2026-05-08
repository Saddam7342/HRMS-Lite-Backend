using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Common.Interfaces;

public interface ITravelRequestRepository : IGenericRepository<TravelRequest>
{
    Task<IReadOnlyList<TravelRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<TravelRequest>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default);
    Task<IReadOnlyList<TravelRequest>> GetTeamScheduleAsync(Guid managerId, DateTime start, DateTime end, CancellationToken ct = default);
    Task<bool> HasOverlappingTravelAsync(Guid employeeId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default);
    Task<TravelRequest?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}
