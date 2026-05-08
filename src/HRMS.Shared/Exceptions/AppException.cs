namespace HRMS.Shared.Exceptions;

/// <summary>
/// Base application exception for all domain/application-level errors.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
