using Asp.Versioning;
using HRMS.Application.Features.Audit.DTOs;
using HRMS.Application.Features.Audit.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class AuditController : BaseApiController
{
    [HttpGet("entity/{entityName}/{entityId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetEntityHistory(string entityName, string entityId)
    {
        var result = await Mediator.Send(new GetEntityAuditHistoryQuery(entityName, entityId));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserActivity(Guid userId, [FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetUserActivityHistoryQuery(userId, limit));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSystemLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await Mediator.Send(new GetSystemAuditLogsQuery(page, pageSize));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetAuditLogByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }
}
