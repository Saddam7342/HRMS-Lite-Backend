using HRMS.Application.Features.Payroll.Commands;
using HRMS.Application.Features.Payroll.DTOs;
using HRMS.Application.Features.Payroll.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize]
public class PayrollController : BaseApiController
{
    [HttpPost("generate")]
    [Authorize(Roles = "OrganizationAdmin")]
    public async Task<IActionResult> Generate([FromBody] GeneratePayrollCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<int>.Ok(result.Data)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayslip([FromQuery] int month, [FromQuery] int year)
    {
        var result = await Mediator.Send(new GetMyPayslipQuery(month, year));
        return result.IsSuccess ? Ok(ApiResponse<PaySlipDto>.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "OrganizationAdmin,Manager")]
    public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year)
    {
        var result = await Mediator.Send(new GetPayrollSummaryQuery(month, year));
        return Ok(ApiResponse<PayrollSummaryDto>.Ok(result.Data!));
    }

    [HttpGet("monthly")]
    [Authorize(Roles = "OrganizationAdmin")]
    public async Task<IActionResult> GetMonthlyPayroll([FromQuery] int month, [FromQuery] int year)
    {
        var result = await Mediator.Send(new GetPayrollByMonthQuery(month, year));
        return Ok(ApiResponse<IReadOnlyList<PayrollDto>>.Ok(result.Data!));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "OrganizationAdmin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApprovePayrollCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPost("salary-structure")]
    [Authorize(Roles = "OrganizationAdmin")]
    public async Task<IActionResult> CreateSalaryStructure([FromBody] CreateSalaryStructureRequest request)
    {
        var result = await Mediator.Send(new CreateSalaryStructureCommand(request));
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
