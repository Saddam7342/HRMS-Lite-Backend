using HRMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRMS.API.Middleware;

public class FeatureFlagMiddleware(RequestDelegate next)
{
    private static readonly Dictionary<string, string> ModuleFlags = new()
    {
        { "/api/v1/leaves", "features.leave.enabled" },
        { "/api/v1/expenses", "features.expense.enabled" },
        { "/api/v1/travel", "features.travel.enabled" },
        { "/api/v1/attendance", "features.attendance.enabled" }
    };

    public async Task InvokeAsync(HttpContext context, ISettingsService settingsService)
    {
        var path = context.Request.Path.Value?.ToLower();
        if (string.IsNullOrEmpty(path))
        {
            await next(context);
            return;
        }

        foreach (var flag in ModuleFlags)
        {
            if (path.StartsWith(flag.Key))
            {
                var isEnabled = await settingsService.IsFeatureEnabledAsync(flag.Value);
                if (!isEnabled)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "This module is disabled for your organization." });
                    return;
                }
                break;
            }
        }

        await next(context);
    }
}
