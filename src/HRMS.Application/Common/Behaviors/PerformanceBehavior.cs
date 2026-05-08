using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that measures handler execution time.
/// Logs a WARNING if a request takes longer than the defined threshold.
/// Useful for catching N+1 queries and slow DB operations early.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int WarningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > WarningThresholdMs)
        {
            logger.LogWarning(
                "[HRMS][SLOW] {RequestName} took {ElapsedMs}ms — Request: {@Request}",
                typeof(TRequest).Name,
                sw.ElapsedMilliseconds,
                request);
        }

        return response;
    }
}
