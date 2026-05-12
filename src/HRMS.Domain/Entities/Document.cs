using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public enum DocumentType
{
    Employee     = 1,
    Company      = 2   // Renamed from Organization to Company
}

public class Document : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public string Category { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid UploadedById { get; set; }
    public AppUser UploadedBy { get; set; } = null!;

    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
