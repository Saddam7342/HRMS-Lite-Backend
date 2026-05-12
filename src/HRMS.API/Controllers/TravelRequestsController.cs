using Asp.Versioning;
using HRMS.API.Models;
using HRMS.Application.Features.Travel.Commands.Approval;
using HRMS.Application.Features.Travel.Commands.Create;
using HRMS.Application.Features.Travel.Commands.Update;
using HRMS.Application.Features.Travel.DTOs;
using HRMS.Application.Features.Travel.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class TravelRequestsController : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllTravelRequestsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateTravelRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyTravels()
    {
        var result = await Mediator.Send(new GetMyTravelRequestsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingTravelApprovalsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("team-schedule")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamSchedule([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await Mediator.Send(new GetTeamTravelScheduleQuery(start, end));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetHistory()
    {
        var result = await Mediator.Send(new GetTravelHistoryQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTravelRequestByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateTravelRequestRequest request)
    {
        var command = new UpdateTravelRequestCommand
        {
            Id = id,
            Destination = request.Destination,
            Purpose = request.Purpose,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            EstimatedBudget = request.EstimatedBudget
        };
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelTravelRequestCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveTravelRequestCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReasonRequest? body)
    {
        var result = await Mediator.Send(new RejectTravelRequestCommand(id, body?.Reason));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }
}
