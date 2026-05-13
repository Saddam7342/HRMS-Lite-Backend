using Asp.Versioning;
using HRMS.API.Models;
using HRMS.Application.Features.Leaves.Commands.Approval;
using HRMS.Application.Features.Leaves.Commands.Cancel;
using HRMS.Application.Features.Leaves.Commands.Create;
using HRMS.Application.Features.Leaves.DTOs;
using HRMS.Application.Features.Leaves.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class LeavesController : BaseApiController
{
    [HttpGet("types")]
    [Authorize]
    public async Task<IActionResult> GetEligibleTypes()
    {
        var result = await Mediator.Send(new GetEligibleLeaveTypesQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllLeaveRequestsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateLeaveRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyLeaves()
    {
        var result = await Mediator.Send(new GetMyLeaveRequestsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("balances")]
    [Authorize]
    public async Task<IActionResult> GetBalances([FromQuery] int? year)
    {
        var result = await Mediator.Send(new GetLeaveBalancesQuery(year));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingLeaveApprovalsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("team-calendar")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamCalendar([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await Mediator.Send(new GetTeamLeaveCalendarQuery(start, end));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveLeaveRequestCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReasonRequest? body)
    {
        var result = await Mediator.Send(new RejectLeaveRequestCommand(id, body?.Reason));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelLeaveRequestCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }
}
