namespace HRMS.Shared.Models;

/// <summary>
/// Generic result wrapper for all service/handler responses.
/// Avoids throwing exceptions for expected failures.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public List<string> Errors { get; private set; } = [];

    protected Result() { }

    public static Result<T> Success(T data, string? message = null) =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static Result<T> Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };

    public static Result<T> Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static Result<T> Failure(string error, string message) =>
        new() { IsSuccess = false, Errors = [error], Message = message };
}

/// <summary>
/// Non-generic result for commands that return no data.
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public List<string> Errors { get; private set; } = [];

    protected Result() { }

    public static Result Success(string? message = null) =>
        new() { IsSuccess = true, Message = message };

    public static Result Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };

    public static Result Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };
}
