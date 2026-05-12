using HRMS.Application.Features.Travel.Commands.Approval;
using HRMS.Application.Features.Travel.Commands.Create;
using HRMS.Application.Features.Travel.Commands.Update;
using HRMS.Application.Features.Travel.DTOs;
using HRMS.Application.Features.Travel.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class TravelRequestsController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateTravelRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyTravels()
    {
        var result = await Mediator.Send(new GetMyTravelRequestsQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTravelRequestByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<TravelRequestDto>.Ok(result.Data!)) : NotFound(ApiResponse<TravelRequestDto>.Fail(result.Errors));
    }

    [HttpPut("{id}")]
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
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelTravelRequestCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingTravelApprovalsQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveTravelRequestCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason)
    {
        var result = await Mediator.Send(new RejectTravelRequestCommand(id, reason));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("team-schedule")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamSchedule(DateTime start, DateTime end)
    {
        var result = await Mediator.Send(new GetTeamTravelScheduleQuery(start, end));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetHistory()
    {
        var result = await Mediator.Send(new GetTravelHistoryQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
