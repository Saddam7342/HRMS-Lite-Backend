using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HRMS.Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    string Title,
    string? Description,
    DocumentType DocumentType,
    string Category,
    Guid? EmployeeId,
    IFormFile File) : IRequest<Result<Guid>>;

public class UploadDocumentHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService) : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        var allowedTypes = new[] { ".pdf", ".jpg", ".png", ".docx" };
        var extension = Path.GetExtension(request.File.FileName).ToLower();
        if (!allowedTypes.Contains(extension))
            return Result<Guid>.Failure("Invalid file type. Allowed: pdf, jpg, png, docx.");

        // 2. Storage Path
        var subDir = request.DocumentType == DocumentType.Employee ? "employees" : "organization";
        var path = $"documents/{tenantContext.TenantId}/{subDir}/{Guid.NewGuid()}{extension}";
        
        using var stream = request.File.OpenReadStream();
        var filePath = await fileStorageService.UploadAsync(stream, path, request.File.ContentType, cancellationToken);

        // 3. Persist Metadata
        var document = new Document
        {
            Title = request.Title,
            Description = request.Description,
            FileName = request.File.FileName,
            FilePath = filePath,
            FileType = extension,
            FileSize = request.File.Length,
            DocumentType = request.DocumentType,
            Category = request.Category,
            EmployeeId = request.EmployeeId,
            UploadedById = currentUserService.UserId ?? Guid.Empty,
            Version = 1,
            IsActive = true
        };

        await unitOfWork.Documents.AddAsync(document, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(document.Id);
    }
}
