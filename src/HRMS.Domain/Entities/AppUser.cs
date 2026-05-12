using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

/// <summary>
/// System user with authentication credentials.
/// Single-company HRMS — no tenant/organization scoping.
/// </summary>
public class AppUser : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsEmailConfirmed { get; set; } = false;
    public bool PasswordResetRequired { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Navigation
    public Employee? Employee { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
