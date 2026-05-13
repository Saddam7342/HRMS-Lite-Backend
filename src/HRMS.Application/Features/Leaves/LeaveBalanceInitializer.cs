using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Leaves;

/// <summary>
/// Ensures each employee has leave balance rows for active leave types (e.g. legacy users or DBs seeded before balances existed).
/// </summary>
public static class LeaveBalanceInitializer
{
    public static async Task EnsureForEmployeeYearAsync(
        IUnitOfWork unitOfWork,
        Guid employeeId,
        Gender employeeGender,
        int year,
        CancellationToken cancellationToken)
    {
        var leaveTypes = await unitOfWork.LeaveTypes.GetAllActiveAsync(cancellationToken);
        var added = false;

        foreach (var lt in leaveTypes)
        {
            if (lt.IsGenderSpecific && lt.ApplicableGender != employeeGender)
                continue;

            var existing = await unitOfWork.LeaveBalances.GetByEmployeeAndTypeAsync(employeeId, lt.Id, year, cancellationToken);
            if (existing != null)
                continue;

            await unitOfWork.LeaveBalances.AddAsync(new LeaveBalance
            {
                EmployeeId = employeeId,
                LeaveTypeId = lt.Id,
                TotalDays = lt.DefaultDays,
                UsedDays = 0,
                Year = year
            }, cancellationToken);
            added = true;
        }

        if (added)
            await unitOfWork.CommitAsync(cancellationToken);
    }
}
