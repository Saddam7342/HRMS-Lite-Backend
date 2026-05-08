using HRMS.Application.Features.Notifications.Commands;
using HRMS.Application.Features.Notifications.DTOs;
using HRMS.Application.Features.Notifications.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class NotificationsController : BaseApiController
{
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetMyNotificationsQuery(page, pageSize));
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result.Data!));
    }

    [HttpGet("count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await Mediator.Send(new GetNotificationCountQuery());
        return Ok(ApiResponse<int>.Ok(result.Data));
    }

    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await Mediator.Send(new MarkAsReadCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("read-all")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await Mediator.Send(new MarkAllAsReadCommand());
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteNotificationCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("preferences")]
    [Authorize]
    public async Task<IActionResult> GetPreferences()
    {
        var result = await Mediator.Send(new GetNotificationPreferencesQuery());
        return Ok(ApiResponse<NotificationPreferencesDto>.Ok(result.Data!));
    }

    [HttpPut("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences(UpdateNotificationPreferencesRequest request)
    {
        var command = new UpdateNotificationPreferencesCommand(
            request.EmailEnabled,
            request.InAppEnabled,
            request.LeaveNotifications,
            request.ExpenseNotifications,
            request.TravelNotifications,
            request.AttendanceNotifications);

        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
