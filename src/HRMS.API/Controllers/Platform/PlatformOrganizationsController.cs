using HRMS.API.Controllers;
using HRMS.Application.Features.Platform.Commands;
using HRMS.Application.Features.Organizations.DTOs;
using HRMS.Application.Features.Organizations.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers.Platform;

[ApiVersion("1.0")]
[Tags("Platform Management")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformOrganizationsController : BaseApiController
{
    /// <summary>
    /// Provisions a new tenant with an admin account and default settings.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
    public async Task<IActionResult> Provision(ProvisionOrganizationCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse<Guid>.Ok(result.Data)) 
            : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    /// <summary>
    /// Lists all organizations across the platform.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetOrganizationsQuery());
        return Ok(ApiResponse<IReadOnlyList<OrganizationDto>>.Ok(result.Data ?? []));
    }
}
