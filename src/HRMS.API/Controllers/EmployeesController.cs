using Asp.Versioning;
using HRMS.Application.Features.Employees.Commands.CreateEmployee;
using HRMS.Application.Features.Employees.Commands.Status;
using HRMS.Application.Features.Employees.Commands.UpdateEmployee;
using HRMS.Application.Features.Employees.Commands.UploadImage;
using HRMS.Application.Features.Employees.DTOs;
using HRMS.Application.Features.Employees.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class EmployeesController : BaseApiController
{
    /// <summary>
    /// Onboards a new employee. Creates AppUser and Employee profile.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    /// <summary>
    /// Gets a paged list of all employees in the organization.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
    {
        var result = await Mediator.Send(new GetEmployeesQuery(paginationParams));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    /// <summary>
    /// Gets the profile of the currently authenticated employee.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetMyProfileQuery());
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    /// <summary>
    /// Gets the list of employees reporting to the current manager (or admin).
    /// </summary>
    [HttpGet("my-team")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TeamMemberDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTeam()
    {
        var me = await Mediator.Send(new GetMyProfileQuery());
        if (!me.IsSuccess)
            return BadData(me);

        var result = await Mediator.Send(new GetTeamMembersQuery(me.Data!.Id));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    /// <summary>
    /// Gets a specific employee's profile by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    /// <summary>
    /// Updates an employee's profile details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeCommand command)
    {
        if (id != command.Id)
            return BadEnvelope("Route id must match body id.");

        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    /// <summary>
    /// Activates an employee's account and profile.
    /// </summary>
    [HttpPut("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateEmployeeCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    /// <summary>
    /// Deactivates an employee's account and profile.
    /// </summary>
    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateEmployeeCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    /// <summary>
    /// Uploads a profile image for an employee.
    /// </summary>
    [HttpPost("{id:guid}/profile-image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadEnvelope("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadEmployeeProfileImageCommand(id, stream, file.FileName, file.ContentType));

        return result.IsSuccess ? OkData(result) : BadData(result);
    }
}
