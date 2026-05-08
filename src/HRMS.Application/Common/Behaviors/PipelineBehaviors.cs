using System.Diagnostics;
using HRMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Common.Behaviors;

public class PerformanceLoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.UserId;

            logger.LogWarning("HRMS Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                requestName, elapsedMilliseconds, userId, request);
        }

        return response;
    }
}

public class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheableQuery.CacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            logger.LogInformation("Cache hit for {QueryName}: {CacheKey}", typeof(TRequest).Name, cacheableQuery.CacheKey);
            return cachedResponse;
        }

        var response = await next();

        await cacheService.SetAsync(cacheableQuery.CacheKey, response, cacheableQuery.Expiration, cancellationToken);
        logger.LogInformation("Cache miss for {QueryName}: {CacheKey}. Value cached.", typeof(TRequest).Name, cacheableQuery.CacheKey);

        return response;
    }
}
