using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "PlatformAdmin")]
public class SystemController(ICacheService cacheService) : BaseApiController
{
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache([FromQuery] string? prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            // Simple clearing for current tenant
            await cacheService.RemoveByPrefixAsync("");
        }
        else
        {
            await cacheService.RemoveByPrefixAsync(prefix);
        }

        return Ok(ApiResponse.Ok("Cache cleared successfully."));
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            System = "HRMS-Lite",
            Version = "1.0.0",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        }));
    }
}
