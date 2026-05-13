using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace HRMS.API.Middleware;

public class RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const int Limit = 500; // Requests
    private const int PeriodSeconds = 60; // Per minute

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetClientKey(context);
        var cacheKey = $"rl_{key}";

        if (cache.TryGetValue(cacheKey, out int count))
        {
            if (count >= Limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new { error = "Too many requests. Please try again later." });
                return;
            }
            cache.Set(cacheKey, count + 1, TimeSpan.FromSeconds(PeriodSeconds));
        }
        else
        {
            cache.Set(cacheKey, 1, TimeSpan.FromSeconds(PeriodSeconds));
        }

        await next(context);
    }

    private string GetClientKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId)) return userId;

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
