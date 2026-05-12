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

public class EmployeesController : BaseApiController
{
    /// <summary>
    /// Onboards a new employee. Creates AppUser and Employee profile.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
    public async Task<IActionResult> Create(CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    /// <summary>
    /// Gets a paged list of all employees in the organization.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeListDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
    {
        var result = await Mediator.Send(new GetEmployeesQuery(paginationParams));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Gets the profile of the currently authenticated employee.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeProfileDto>), 200)]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetMyProfileQuery());
        return result.IsSuccess ? Ok(ApiResponse<EmployeeProfileDto>.Ok(result.Data!)) : NotFound(ApiResponse<EmployeeProfileDto>.Fail(result.Errors));
    }

    /// <summary>
    /// Gets a specific employee's profile by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeProfileDto>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<EmployeeProfileDto>.Ok(result.Data!)) : NotFound(ApiResponse<EmployeeProfileDto>.Fail(result.Errors));
    }

    /// <summary>
    /// Updates an employee's profile details.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse.Fail("ID mismatch."));
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Activates an employee's account and profile.
    /// </summary>
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateEmployeeCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Deactivates an employee's account and profile.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateEmployeeCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Gets the list of employees reporting to a specific manager.
    /// </summary>
    [HttpGet("my-team")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TeamMemberDto>>), 200)]
    public async Task<IActionResult> GetMyTeam()
    {
        // For managers, we fetch using their own employee ID
        var me = await Mediator.Send(new GetMyProfileQuery());
        if (!me.IsSuccess) return BadRequest(ApiResponse.Fail(me.Errors));
        
        var result = await Mediator.Send(new GetTeamMembersQuery(me.Data!.Id));
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Uploads a profile image for an employee.
    /// </summary>
    [HttpPost("{id}/profile-image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse.Fail("No file uploaded."));
        
        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadEmployeeProfileImageCommand(id, stream, file.FileName, file.ContentType));
        
        return result.IsSuccess ? Ok(ApiResponse<string>.Ok(result.Data!)) : BadRequest(ApiResponse<string>.Fail(result.Errors));
    }
}
