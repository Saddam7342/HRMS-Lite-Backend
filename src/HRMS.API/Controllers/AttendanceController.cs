using Asp.Versioning;
using HRMS.Application.Features.Attendance.Commands.CheckIn;
using HRMS.Application.Features.Attendance.Commands.CheckOut;
using HRMS.Application.Features.Attendance.Commands.Update;
using HRMS.Application.Features.Attendance.DTOs;
using HRMS.Application.Features.Attendance.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class AttendanceController : BaseApiController
{
    [HttpPost("check-in")]
    [Authorize]
    public async Task<IActionResult> CheckIn(CheckInRequest request)
    {
        var result = await Mediator.Send(new CheckInCommand(request.Notes));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPost("check-out")]
    [Authorize]
    public async Task<IActionResult> CheckOut(CheckOutRequest request)
    {
        var result = await Mediator.Send(new CheckOutCommand(request.Notes));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await Mediator.Send(new GetMyAttendanceQuery(start, end));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("today")]
    [Authorize]
    public async Task<IActionResult> GetToday()
    {
        var result = await Mediator.Send(new GetTodayAttendanceQuery());
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    [HttpGet("range")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRange([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await Mediator.Send(new GetAttendanceByDateRangeQuery(start, end));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("team")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeam([FromQuery] DateTime date)
    {
        var result = await Mediator.Send(new GetTeamAttendanceQuery(date));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await Mediator.Send(new GetAttendanceSummaryQuery(start, end));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateAttendanceCommand command)
    {
        if (id != command.Id)
            return BadEnvelope("Route id must match body id.");

        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/mark-absent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAbsent(Guid id, [FromQuery] DateTime date)
    {
        var result = await Mediator.Send(new MarkAttendanceAbsentCommand(id, date));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }
}
