using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Expenses.Commands.Create;

public record CreateExpenseClaimCommand : IRequest<Result<Guid>>
{
    public Guid CategoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public DateTime ExpenseDate { get; init; }
}

public class CreateExpenseClaimValidator : AbstractValidator<CreateExpenseClaimCommand>
{
    public CreateExpenseClaimValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpenseDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public class CreateExpenseClaimHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateExpenseClaimCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExpenseClaimCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<Guid>.Failure("Unauthorized.");

        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId.Value, cancellationToken);
        if (employee == null) return Result<Guid>.Failure("Employee profile not found.");

        var category = await unitOfWork.ExpenseCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null || !category.IsActive)
            return Result<Guid>.Failure("Invalid or inactive expense category.");

        var claim = new ExpenseClaim
        {
            EmployeeId = employee.Id,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Status = ExpenseClaimStatus.Pending,
            SubmittedAt = dateTimeProvider.UtcNow
        };

        await unitOfWork.ExpenseClaims.AddAsync(claim, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(claim.Id);
    }
}
