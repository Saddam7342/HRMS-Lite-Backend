using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Expenses.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Expenses.Queries;

public record GetMyExpenseClaimsQuery : IRequest<Result<IReadOnlyList<ExpenseClaimDto>>>;
public record GetPendingExpenseApprovalsQuery : IRequest<Result<IReadOnlyList<ExpenseClaimDto>>>;
public record GetTeamExpenseClaimsQuery : IRequest<Result<IReadOnlyList<ExpenseClaimListDto>>>;
public record GetExpenseCategoriesQuery : IRequest<Result<IReadOnlyList<ExpenseCategoryDto>>>;
public record GetExpenseClaimByIdQuery(Guid Id) : IRequest<Result<ExpenseClaimDto>>;

public class ExpenseQueryHandlers(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper) 
    : IRequestHandler<GetMyExpenseClaimsQuery, Result<IReadOnlyList<ExpenseClaimDto>>>,
      IRequestHandler<GetPendingExpenseApprovalsQuery, Result<IReadOnlyList<ExpenseClaimDto>>>,
      IRequestHandler<GetTeamExpenseClaimsQuery, Result<IReadOnlyList<ExpenseClaimListDto>>>,
      IRequestHandler<GetExpenseCategoriesQuery, Result<IReadOnlyList<ExpenseCategoryDto>>>,
      IRequestHandler<GetExpenseClaimByIdQuery, Result<ExpenseClaimDto>>
{
    public async Task<Result<IReadOnlyList<ExpenseClaimDto>>> Handle(GetMyExpenseClaimsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<ExpenseClaimDto>>.Failure("Employee not found.");

        var claims = await unitOfWork.ExpenseClaims.GetByEmployeeAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<ExpenseClaimDto>>.Success(mapper.Map<IReadOnlyList<ExpenseClaimDto>>(claims));
    }

    public async Task<Result<IReadOnlyList<ExpenseClaimDto>>> Handle(GetPendingExpenseApprovalsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<ExpenseClaimDto>>.Failure("Employee not found.");

        var pending = await unitOfWork.ExpenseClaims.GetPendingByManagerAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<ExpenseClaimDto>>.Success(mapper.Map<IReadOnlyList<ExpenseClaimDto>>(pending));
    }

    public async Task<Result<IReadOnlyList<ExpenseClaimListDto>>> Handle(GetTeamExpenseClaimsQuery request, CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee == null) return Result<IReadOnlyList<ExpenseClaimListDto>>.Failure("Employee not found.");

        var teamClaims = await unitOfWork.ExpenseClaims.GetTeamClaimsAsync(employee.Id, cancellationToken);
        return Result<IReadOnlyList<ExpenseClaimListDto>>.Success(mapper.Map<IReadOnlyList<ExpenseClaimListDto>>(teamClaims));
    }

    public async Task<Result<IReadOnlyList<ExpenseCategoryDto>>> Handle(GetExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.ExpenseCategories.GetAllActiveAsync(cancellationToken);
        return Result<IReadOnlyList<ExpenseCategoryDto>>.Success(mapper.Map<IReadOnlyList<ExpenseCategoryDto>>(categories));
    }

    public async Task<Result<ExpenseClaimDto>> Handle(GetExpenseClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = await unitOfWork.ExpenseClaims.GetWithDetailsAsync(request.Id, cancellationToken);
        if (claim == null) return Result<ExpenseClaimDto>.Failure("Claim not found.");

        return Result<ExpenseClaimDto>.Success(mapper.Map<ExpenseClaimDto>(claim));
    }

    private async Task<Domain.Entities.Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue) return null;
        return await unitOfWork.Employees.GetByUserIdAsync(currentUserService.UserId.Value, ct);
    }
}
