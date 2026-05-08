using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HRMS.Application.Features.Documents.Commands;

public record DeleteDocumentCommand(Guid Id) : IRequest<Result>;
public record UpdateDocumentCommand(Guid Id, string Title, string? Description, string Category) : IRequest<Result>;
public record UploadNewVersionCommand(Guid Id, IFormFile File) : IRequest<Result>;

public class DocumentMaintenanceHandlers(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ITenantContext tenantContext) 
    : IRequestHandler<DeleteDocumentCommand, Result>,
      IRequestHandler<UpdateDocumentCommand, Result>,
      IRequestHandler<UploadNewVersionCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return Result.Failure("Document not found.");

        document.IsActive = false; // Soft delete
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return Result.Failure("Document not found.");

        document.Title = request.Title;
        document.Description = request.Description;
        document.Category = request.Category;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(UploadNewVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return Result.Failure("Document not found.");

        var extension = Path.GetExtension(request.File.FileName).ToLower();
        if (extension != document.FileType)
            return Result.Failure($"New version must have the same file type ({document.FileType}).");

        // 1. Upload new file
        var subDir = document.DocumentType == DocumentType.Employee ? "employees" : "organization";
        var path = $"documents/{tenantContext.TenantId}/{subDir}/{Guid.NewGuid()}{extension}";
        
        using var stream = request.File.OpenReadStream();
        var newPath = await fileStorageService.UploadAsync(stream, path, request.File.ContentType, cancellationToken);

        // 2. Update metadata
        document.FilePath = newPath;
        document.FileName = request.File.FileName;
        document.FileSize = request.File.Length;
        document.Version++;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
