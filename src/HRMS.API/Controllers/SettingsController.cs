using HRMS.Application.Features.Settings.Commands;
using HRMS.Application.Features.Settings.DTOs;
using HRMS.Application.Features.Settings.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "OrganizationAdmin")]
public class SettingsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllSettingsQuery());
        return Ok(ApiResponse<IReadOnlyList<OrganizationSettingDto>>.Ok(result.Data!));
    }

    [HttpGet("module/{module}")]
    public async Task<IActionResult> GetByModule(string module)
    {
        var result = await Mediator.Send(new GetSettingsByModuleQuery(module));
        return Ok(ApiResponse<IReadOnlyList<OrganizationSettingDto>>.Ok(result.Data!));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateOrganizationSettingCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateSettingsCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
