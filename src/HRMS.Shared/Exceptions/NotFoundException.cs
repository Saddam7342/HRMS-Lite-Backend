namespace HRMS.Shared.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist.
/// Maps to HTTP 404 in the global exception handler.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}
