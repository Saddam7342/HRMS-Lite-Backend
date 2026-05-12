using HRMS.Application.Features.Reports.DTOs;
using HRMS.Application.Features.Reports.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ReportsController : BaseApiController
{
    [HttpGet("hr-dashboard")]
    public async Task<IActionResult> GetHrDashboard()
    {
        var result = await Mediator.Send(new GetHrDashboardSummaryQuery());
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("leaves")]
    public async Task<IActionResult> GetLeaveAnalytics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var result = await Mediator.Send(new GetLeaveAnalyticsQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenseAnalytics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var result = await Mediator.Send(new GetExpenseAnalyticsQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceAnalytics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var result = await Mediator.Send(new GetAttendanceAnalyticsQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
