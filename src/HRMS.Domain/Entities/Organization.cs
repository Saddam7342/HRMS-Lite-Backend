using HRMS.Domain.Common;
using HRMS.Domain.Common.Interfaces;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents an organization (tenant) in the system.
/// Required fields as per module requirements.
/// </summary>
public class Organization : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;           // Unique, used for tenant identification
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; } = "#3f51b5";      // Default branding
    public string? SecondaryColor { get; set; } = "#f50057";
    public int MaxEmployeeSlots { get; set; } = 10;            // Subscription/Limit foundation
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }                         // For root isolation consistency

    // Navigation
    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<Department> Departments { get; set; } = [];
}
