using System.Net;
using System.Text.Json;
using HRMS.Shared.Models;
using HRMS.Shared.Exceptions;

namespace HRMS.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;
        var message = "An internal server error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case ValidationException validationEx:
                code = HttpStatusCode.UnprocessableEntity;
                message = validationEx.Message;
                errors = validationEx.ValidationErrors.SelectMany(x => x.Value).ToList();
                break;
            case NotFoundException notFoundEx:
                code = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;
            case UnauthorizedException:
                code = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;
            case ForbiddenException:
                code = HttpStatusCode.Forbidden;
                message = "You do not have permission to access this resource.";
                break;
            case AppException appEx:
                code = (HttpStatusCode)appEx.StatusCode;
                message = appEx.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var response = ApiResponse<object>.Fail(errors ?? [message], message, context.TraceIdentifier);
        
        result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(result);
    }
}
