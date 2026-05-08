using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public enum DocumentType
{
    Employee = 1,
    Organization = 2
}

public class Document : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // .pdf, .docx, etc.
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public string Category { get; set; } = string.Empty; // ID, Contract, Policy, etc.
    
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    
    public Guid UploadedById { get; set; }
    public AppUser UploadedBy { get; set; } = null!;
    
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
