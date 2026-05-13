using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the start and completion
/// (or failure) of every command and query request.
/// Runs before ValidationBehavior in the pipeline.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Avoid serializing full request payloads — large DTOs add CPU/allocations on every API call.
        logger.LogInformation("[HRMS] Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            logger.LogInformation("[HRMS] Handled {RequestName} successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[HRMS] Error handling {RequestName}", requestName);
            throw;
        }
    }
}
