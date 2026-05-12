using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Documents.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Documents.Queries;

public record GetDocumentByIdQuery(Guid Id) : IRequest<Result<DocumentDto>>;
public record GetEmployeeDocumentsQuery(Guid EmployeeId) : IRequest<Result<IReadOnlyList<DocumentDto>>>;
public record GetCompanyDocumentsQuery() : IRequest<Result<IReadOnlyList<DocumentDto>>>;
public record DownloadDocumentQuery(Guid Id) : IRequest<Result<FileDownloadModel>>;

public record FileDownloadModel(Stream FileStream, string ContentType, string FileName);

public class DocumentQueryHandlers(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IMapper mapper,
    ICurrentUserService currentUserService) 
    : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>,
      IRequestHandler<GetEmployeeDocumentsQuery, Result<IReadOnlyList<DocumentDto>>>,
      IRequestHandler<GetCompanyDocumentsQuery, Result<IReadOnlyList<DocumentDto>>>,
      IRequestHandler<DownloadDocumentQuery, Result<FileDownloadModel>>
{
    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Documents.GetQueryable()
            .Include(x => x.Employee)
            .Include(x => x.UploadedBy)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (document == null) return Result<DocumentDto>.Failure("Document not found.");

        if (!CanAccessDocument(document)) return Result<DocumentDto>.Failure("Unauthorized access.");

        return Result<DocumentDto>.Success(mapper.Map<DocumentDto>(document));
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> Handle(GetEmployeeDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.Roles.Contains("Employee") && !currentUserService.Roles.Contains("Manager") && !currentUserService.Roles.Contains("Admin"))
        {
            var userId = currentUserService.UserId ?? Guid.Empty;
            var employee = await unitOfWork.Employees.GetByUserIdAsync(userId, cancellationToken);
            if (employee == null || employee.Id != request.EmployeeId)
                return Result<IReadOnlyList<DocumentDto>>.Failure("Unauthorized access.");
        }

        var docs = await unitOfWork.Documents.GetByEmployeeAsync(request.EmployeeId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success(mapper.Map<IReadOnlyList<DocumentDto>>(docs));
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> Handle(GetCompanyDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await unitOfWork.Documents.GetCompanyDocumentsAsync(cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success(mapper.Map<IReadOnlyList<DocumentDto>>(docs));
    }

    public async Task<Result<FileDownloadModel>> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (document == null || !document.IsActive) return Result<FileDownloadModel>.Failure("Document not found.");

        if (!CanAccessDocument(document)) return Result<FileDownloadModel>.Failure("Unauthorized access.");

        var stream = await fileStorageService.DownloadAsync(document.FilePath, cancellationToken);
        var contentType = GetContentType(document.FileType);

        return Result<FileDownloadModel>.Success(new FileDownloadModel(stream, contentType, document.FileName));
    }

    private bool CanAccessDocument(Document doc)
    {
        if (currentUserService.Roles.Contains("Admin")) return true;
        if (doc.DocumentType == DocumentType.Company) return true;
        if (currentUserService.Roles.Contains("Manager")) return true;

        return true; 
    }

    private string GetContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".jpg" => "image/jpeg",
        ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };
}
