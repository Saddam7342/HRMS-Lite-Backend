namespace HRMS.Shared.Constants;

/// <summary>
/// JWT claim type key names. Centralized to avoid typo-prone magic strings.
/// Single-company HRMS — no TenantId claim.
/// </summary>
public static class AppClaimTypes
{
    public const string UserId     = "uid";
    public const string Role       = "role";
    public const string Email      = "email";
    public const string FullName   = "name";
    public const string Username   = "username";
    public const string Permission = "perm";
}
