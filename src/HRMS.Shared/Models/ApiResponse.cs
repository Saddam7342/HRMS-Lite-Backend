namespace HRMS.Shared.Models;

/// <summary>
/// Standardized HTTP response envelope returned from all API endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public List<string>? Errors { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            TraceId = traceId ?? string.Empty
        };

    public static ApiResponse<T> Fail(List<string> errors, string? message = null, string? traceId = null) =>
        new()
        {
            Success = false,
            Errors = errors,
            Message = message,
            TraceId = traceId ?? string.Empty
        };

    public static ApiResponse<T> Fail(string error, string? message = null, string? traceId = null) =>
        Fail([error], message, traceId);
}

/// <summary>
/// Non-generic API response for commands returning no data payload.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null, string? traceId = null) =>
        new() { Success = true, Message = message, TraceId = traceId ?? string.Empty };

    public static ApiResponse Fail(List<string> errors, string? message = null, string? traceId = null) =>
        new() { Success = false, Errors = errors, Message = message, TraceId = traceId ?? string.Empty };

    public static ApiResponse Fail(string error, string? traceId = null) =>
        Fail([error], null, traceId);
}
