namespace HRMS.Shared.Exceptions;

/// <summary>
/// Thrown when a user is unauthenticated.
/// Maps to HTTP 401 in the global exception handler.
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string? message = null)
        : base(message ?? "Authentication is required to access this resource.", 401)
    {
    }
}
