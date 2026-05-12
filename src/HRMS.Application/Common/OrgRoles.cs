namespace HRMS.Application.Common;

/// <summary>
/// Role helpers — seeded role is <c>Admin</c>; legacy docs refer to <c>OrganizationAdmin</c>.
/// </summary>
public static class OrgRoles
{
    public static bool IsCompanyAdmin(IReadOnlyList<string> roles) =>
        roles.Contains("Admin") ||
        roles.Contains("OrganizationAdmin") ||
        roles.Contains("PlatformAdmin");
}
