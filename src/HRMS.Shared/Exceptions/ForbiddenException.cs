namespace HRMS.Shared.Exceptions;

/// <summary>
/// Thrown when a user attempts an action they are not authorized to perform.
/// Maps to HTTP 403 in the global exception handler.
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.", 403)
    {
    }
}
