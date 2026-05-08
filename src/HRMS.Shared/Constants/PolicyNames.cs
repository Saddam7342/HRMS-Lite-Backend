namespace HRMS.Shared.Constants;

/// <summary>
/// Authorization policy name constants. Referenced in both
/// Infrastructure (registration) and Controllers (enforcement).
/// </summary>
public static class PolicyNames
{
    public const string RequireOrgAdmin = "RequireOrgAdmin";
    public const string RequireManager  = "RequireManager";
    public const string RequireEmployee = "RequireEmployee";
    public const string RequireTenant   = "RequireTenant";
}
