using Asp.Versioning;
using HRMS.Application.Features.Departments.Commands.Create;
using HRMS.Application.Features.Departments.Commands.Status;
using HRMS.Application.Features.Departments.Commands.Update;
using HRMS.Application.Features.Departments.DTOs;
using HRMS.Application.Features.Departments.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class DepartmentsController : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateDepartmentCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetDepartmentsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("hierarchy")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentHierarchyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHierarchy()
    {
        var result = await Mediator.Send(new GetDepartmentHierarchyQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetDepartmentByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    [HttpGet("{id:guid}/employees")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentEmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(Guid id)
    {
        var result = await Mediator.Send(new GetDepartmentEmployeesQuery(id));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentCommand command)
    {
        if (id != command.Id)
            return BadEnvelope("Route id must match body id.");

        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDepartmentCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateDepartmentCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateDepartmentCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }
}
