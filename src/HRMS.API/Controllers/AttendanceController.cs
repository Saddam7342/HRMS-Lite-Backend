using HRMS.Application.Features.Attendance.Commands.CheckIn;
using HRMS.Application.Features.Attendance.Commands.CheckOut;
using HRMS.Application.Features.Attendance.Commands.Update;
using HRMS.Application.Features.Attendance.DTOs;
using HRMS.Application.Features.Attendance.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class AttendanceController : BaseApiController
{
    [HttpPost("check-in")]
    [Authorize]
    public async Task<IActionResult> CheckIn(CheckInRequest request)
    {
        var result = await Mediator.Send(new CheckInCommand(request.Notes));
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    [HttpPost("check-out")]
    [Authorize]
    public async Task<IActionResult> CheckOut(CheckOutRequest request)
    {
        var result = await Mediator.Send(new CheckOutCommand(request.Notes));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyAttendance(DateTime start, DateTime end)
    {
        var result = await Mediator.Send(new GetMyAttendanceQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("today")]
    [Authorize]
    public async Task<IActionResult> GetToday()
    {
        var result = await Mediator.Send(new GetTodayAttendanceQuery());
        return result.IsSuccess ? Ok(ApiResponse<AttendanceDto>.Ok(result.Data!)) : NotFound(ApiResponse<AttendanceDto>.Fail(result.Errors));
    }

    [HttpGet("range")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRange(DateTime start, DateTime end)
    {
        var result = await Mediator.Send(new GetAttendanceByDateRangeQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("team")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeam(DateTime date)
    {
        var result = await Mediator.Send(new GetTeamAttendanceQuery(date));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateAttendanceCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse.Fail("ID mismatch."));
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/mark-absent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAbsent(Guid id, [FromQuery] DateTime date)
    {
        var result = await Mediator.Send(new MarkAttendanceAbsentCommand(id, date));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary(DateTime start, DateTime end)
    {
        var result = await Mediator.Send(new GetAttendanceSummaryQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
