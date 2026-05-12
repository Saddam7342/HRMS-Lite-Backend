using HRMS.Application.Features.Leaves.Commands.Approval;
using HRMS.Application.Features.Leaves.Commands.Cancel;
using HRMS.Application.Features.Leaves.Commands.Create;
using HRMS.Application.Features.Leaves.DTOs;
using HRMS.Application.Features.Leaves.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class LeavesController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateLeaveRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyLeaves()
    {
        var result = await Mediator.Send(new GetMyLeaveRequestsQuery());
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("balances")]
    [Authorize]
    public async Task<IActionResult> GetBalances(int? year)
    {
        var result = await Mediator.Send(new GetLeaveBalancesQuery(year));
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingLeaveApprovalsQuery());
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveLeaveRequestCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason)
    {
        var result = await Mediator.Send(new RejectLeaveRequestCommand(id, reason));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelLeaveRequestCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("team-calendar")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamCalendar(DateTime start, DateTime end)
    {
        var result = await Mediator.Send(new GetTeamLeaveCalendarQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.OkData(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
