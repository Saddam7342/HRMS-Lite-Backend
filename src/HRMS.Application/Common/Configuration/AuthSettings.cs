namespace HRMS.Application.Common.Configuration;

/// <summary>
/// Optional development-only auth shortcuts. Do not set <see cref="DevLoginBypassPassword"/> in production.
/// </summary>
public class AuthSettings
{
    public const string SectionName = "Auth";

    /// <summary>
    /// When non-empty, a login request whose password equals this value will succeed for the matching user
    /// (same as correct password), ignoring lockout and real password verification.
    /// </summary>
    public string? DevLoginBypassPassword { get; set; }
}
