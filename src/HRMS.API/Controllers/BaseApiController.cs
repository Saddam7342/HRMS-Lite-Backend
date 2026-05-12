using HRMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Correlation id from <c>X-Correlation-ID</c> middleware, or ASP.NET Core trace identifier.
    /// </summary>
    protected string RequestTraceId =>
        HttpContext.Items.TryGetValue("X-Correlation-ID", out var obj) && obj is string cid &&
        !string.IsNullOrWhiteSpace(cid)
            ? cid
            : HttpContext.TraceIdentifier;

    protected static string? FailureMessage(IReadOnlyList<string> errors, string? message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            return message;
        if (errors.Count == 0)
            return null;
        return errors.Count == 1 ? errors[0] : string.Join("; ", errors);
    }

    protected ObjectResult OkData<T>(Result<T> result) =>
        Ok(ApiResponse<T>.Ok(result.Data!, result.Message, RequestTraceId));

    protected ObjectResult OkEmpty(Result result) =>
        Ok(ApiResponse.Ok(result.Message, RequestTraceId));

    protected ObjectResult BadData<T>(Result<T> result) =>
        BadRequest(ApiResponse<T>.Fail(result.Errors, FailureMessage(result.Errors, result.Message),
            RequestTraceId));

    protected ObjectResult BadEmpty(Result result) =>
        BadRequest(ApiResponse.Fail(result.Errors, FailureMessage(result.Errors, result.Message),
            RequestTraceId));

    protected ObjectResult NotFoundData<T>(Result<T> result) =>
        NotFound(ApiResponse<T>.Fail(result.Errors, FailureMessage(result.Errors, result.Message),
            RequestTraceId));

    protected ObjectResult NotFoundEmpty(Result result) =>
        NotFound(ApiResponse.Fail(result.Errors, FailureMessage(result.Errors, result.Message),
            RequestTraceId));

    protected ObjectResult UnauthorizedData<T>(Result<T> result) =>
        Unauthorized(ApiResponse<T>.Fail(result.Errors, FailureMessage(result.Errors, result.Message),
            RequestTraceId));

    protected ObjectResult OkEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message, RequestTraceId));

    protected ObjectResult BadEnvelope<T>(List<string> errors, string? message = null) =>
        BadRequest(ApiResponse<T>.Fail(errors, FailureMessage(errors, message), RequestTraceId));

    protected ObjectResult BadEnvelope(string error) =>
        BadRequest(ApiResponse.Fail(error, RequestTraceId));
}
