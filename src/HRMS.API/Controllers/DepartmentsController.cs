using HRMS.Application.Features.Departments.Commands.Create;
using HRMS.Application.Features.Departments.Commands.Status;
using HRMS.Application.Features.Departments.Commands.Update;
using HRMS.Application.Features.Departments.DTOs;
using HRMS.Application.Features.Departments.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class DepartmentsController : BaseApiController
{
    /// <summary>
    /// Creates a new department within the organization.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
    public async Task<IActionResult> Create(CreateDepartmentCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    /// <summary>
    /// Gets all departments in the organization.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentListDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetDepartmentsQuery());
        return Ok(ApiResponse<IReadOnlyList<DepartmentListDto>>.Ok(result.Data!));
    }

    /// <summary>
    /// Gets a full hierarchical tree of the organization's departments.
    /// </summary>
    [HttpGet("hierarchy")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentHierarchyDto>>), 200)]
    public async Task<IActionResult> GetHierarchy()
    {
        var result = await Mediator.Send(new GetDepartmentHierarchyQuery());
        return Ok(ApiResponse<List<DepartmentHierarchyDto>>.Ok(result.Data!));
    }

    /// <summary>
    /// Gets a specific department's details by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetDepartmentByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<DepartmentDto>.Ok(result.Data!)) : NotFound(ApiResponse<DepartmentDto>.Fail(result.Errors));
    }

    /// <summary>
    /// Updates a department's details. Prevents circular references.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse.Fail("ID mismatch."));
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Deletes a department if no employees are assigned to it.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDepartmentCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Activates a department.
    /// </summary>
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateDepartmentCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Deactivates a department.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateDepartmentCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Gets the list of employees assigned to a specific department.
    /// </summary>
    [HttpGet("{id}/employees")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentEmployeeDto>>), 200)]
    public async Task<IActionResult> GetEmployees(Guid id)
    {
        var result = await Mediator.Send(new GetDepartmentEmployeesQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<IReadOnlyList<DepartmentEmployeeDto>>.Ok(result.Data!)) : BadRequest(ApiResponse<IReadOnlyList<DepartmentEmployeeDto>>.Fail(result.Errors));
    }
}
