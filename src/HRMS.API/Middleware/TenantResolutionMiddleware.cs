using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Constants;

namespace HRMS.API.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // 1. Try resolve from header
        if (context.Request.Headers.TryGetValue(AppConstants.Headers.TenantId, out var tenantIdStr))
        {
            if (Guid.TryParse(tenantIdStr, out var tenantId))
            {
                tenantContext.SetTenant(tenantId);
            }
        }
        // 2. Try resolve from Claims (if authenticated)
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var claim = context.User.FindFirst(AppClaimTypes.TenantId);
            if (claim != null && Guid.TryParse(claim.Value, out var tenantId))
            {
                tenantContext.SetTenant(tenantId);
            }
        }

        // Add to response header for visibility
        if (tenantContext.IsResolved)
        {
            context.Response.Headers[AppConstants.Headers.TenantId] = tenantContext.TenantId.ToString();
        }

        await next(context);
    }
}
