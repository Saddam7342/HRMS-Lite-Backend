namespace HRMS.Shared.Exceptions;

/// <summary>
/// Thrown when FluentValidation pipeline behavior catches invalid input.
/// Maps to HTTP 422 in the global exception handler.
/// </summary>
public class ValidationException : AppException
{
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 422)
    {
        ValidationErrors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { { field, [error] } })
    {
    }
}
