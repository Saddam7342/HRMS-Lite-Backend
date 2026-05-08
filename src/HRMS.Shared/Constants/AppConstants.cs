namespace HRMS.Shared.Constants;

/// <summary>
/// Application-wide constants. Centralized here to avoid magic strings.
/// </summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string OrgAdmin = "OrgAdmin";
        public const string Manager  = "Manager";
        public const string Employee = "Employee";
    }

    public static class Headers
    {
        public const string TenantId    = "X-Tenant-ID";
        public const string CorrelationId = "X-Correlation-ID";
    }

    public static class Cache
    {
        public const int DefaultExpiryMinutes = 30;
    }

    public static class Pagination
    {
        public const int DefaultPage     = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize     = 100;
    }

    public static class Jwt
    {
        public const string Issuer   = "HRMS";
        public const string Audience = "HRMS-Clients";
    }
}
