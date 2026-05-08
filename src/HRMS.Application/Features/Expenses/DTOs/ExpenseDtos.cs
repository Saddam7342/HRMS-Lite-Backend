using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Expenses.DTOs;

public record ExpenseClaimDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string? Description,
    decimal Amount,
    DateTime ExpenseDate,
    ExpenseClaimStatus Status,
    string? ReceiptFileUrl,
    DateTime? SubmittedAt,
    string? ApproverName,
    DateTime? ApprovedAt,
    string? RejectionReason);

public record ExpenseClaimListDto(
    Guid Id,
    string EmployeeName,
    string CategoryName,
    string Title,
    decimal Amount,
    DateTime ExpenseDate,
    ExpenseClaimStatus Status);

public record ExpenseCategoryDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public record CreateExpenseClaimRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    decimal Amount,
    DateTime ExpenseDate);

public record UpdateExpenseClaimRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    decimal Amount,
    DateTime ExpenseDate);
