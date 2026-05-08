using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace HRMS.Application.Features.Documents.DTOs;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    string FileName,
    string FileType,
    long FileSize,
    DocumentType DocumentType,
    string Category,
    Guid? EmployeeId,
    string? EmployeeName,
    Guid UploadedById,
    string UploadedByName,
    int Version,
    DateTime CreatedAt);

public record UploadDocumentRequest(
    string Title,
    string? Description,
    DocumentType DocumentType,
    string Category,
    Guid? EmployeeId,
    IFormFile File);

public record UpdateDocumentRequest(
    string Title,
    string? Description,
    string Category);
