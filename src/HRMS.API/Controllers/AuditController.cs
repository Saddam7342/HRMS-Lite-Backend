using HRMS.Application.Features.Audit.DTOs;
using HRMS.Application.Features.Audit.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class AuditController : BaseApiController
{
    [HttpGet("entity/{entityName}/{entityId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetEntityHistory(string entityName, string entityId)
    {
        var result = await Mediator.Send(new GetEntityAuditHistoryQuery(entityName, entityId));
        return Ok(ApiResponse<IReadOnlyList<AuditLogDto>>.Ok(result.Data!));
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserActivity(Guid userId, [FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetUserActivityHistoryQuery(userId, limit));
        return result.IsSuccess ? Ok(ApiResponse<IReadOnlyList<AuditLogDto>>.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSystemLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await Mediator.Send(new GetSystemAuditLogsQuery(page, pageSize));
        return result.IsSuccess ? Ok(ApiResponse<IReadOnlyList<AuditLogDto>>.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetAuditLogByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<AuditLogDto>.Ok(result.Data!)) : NotFound(ApiResponse.Fail(result.Errors));
    }
}
