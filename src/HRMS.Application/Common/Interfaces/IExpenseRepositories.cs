using HRMS.Application.Common.Interfaces.Repositories;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IExpenseClaimRepository : IGenericRepository<ExpenseClaim>
{
    Task<IReadOnlyList<ExpenseClaim>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseClaim>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseClaim>> GetTeamClaimsAsync(Guid managerId, CancellationToken ct = default);
    Task<ExpenseClaim?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseClaim>> GetAllWithDetailsAsync(CancellationToken ct = default);
}

public interface IExpenseCategoryRepository : IGenericRepository<ExpenseCategory>
{
    Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}
