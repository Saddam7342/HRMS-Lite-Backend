using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Expenses.Commands.Receipt;

public record UploadExpenseReceiptCommand(Guid Id, Stream FileStream, string FileName, string ContentType) : IRequest<Result<string>>;

public class UploadExpenseReceiptHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICurrentUserService currentUserService) : IRequestHandler<UploadExpenseReceiptCommand, Result<string>>
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public async Task<Result<string>> Handle(UploadExpenseReceiptCommand request, CancellationToken cancellationToken)
    {
        var claim = await unitOfWork.ExpenseClaims.GetByIdAsync(request.Id, cancellationToken);
        if (claim == null) return Result<string>.Failure("Expense claim not found.");

        // Check ownership
        var userId = currentUserService.UserId;
        var employee = await unitOfWork.Employees.GetByUserIdAsync(userId ?? Guid.Empty, cancellationToken);
        if (employee == null || claim.EmployeeId != employee.Id)
            return Result<string>.Failure("Unauthorized.");

        if (claim.Status != ExpenseClaimStatus.Pending)
            return Result<string>.Failure("Receipts can only be uploaded for pending claims.");

        // Validation
        var extension = Path.GetExtension(request.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
            return Result<string>.Failure("Invalid file type. Allowed: JPG, PNG, PDF.");

        if (request.FileStream.Length > MaxFileSize)
            return Result<string>.Failure("File size exceeds 5MB limit.");

        // Cleanup old receipt if exists
        if (!string.IsNullOrEmpty(claim.ReceiptFileUrl))
        {
            await fileStorageService.DeleteAsync(claim.ReceiptFileUrl, cancellationToken);
        }

        // Upload new
        var fileName = $"receipt_{claim.Id}{extension}";
        // IFileStorageService doesn't take folder in current implementation, it's implementation dependent or prefix is in filename
        // But usually we want folders. Let's see infrastructure implementation.
        
        var url = await fileStorageService.UploadAsync(request.FileStream, fileName, request.ContentType, cancellationToken);

        claim.ReceiptFileUrl = url;
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<string>.Success(url);
    }
}
