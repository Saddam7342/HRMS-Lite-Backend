using HRMS.Application.Features.Expenses.Commands.Approval;
using HRMS.Application.Features.Expenses.Commands.Create;
using HRMS.Application.Features.Expenses.Commands.Receipt;
using HRMS.Application.Features.Expenses.DTOs;
using HRMS.Application.Features.Expenses.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class ExpenseClaimsController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateExpenseClaimCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data!)) : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyClaims()
    {
        var result = await Mediator.Send(new GetMyExpenseClaimsQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetExpenseClaimByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<ExpenseClaimDto>.Ok(result.Data!)) : NotFound(ApiResponse<ExpenseClaimDto>.Fail(result.Errors));
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingExpenseApprovalsQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("team")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamClaims()
    {
        var result = await Mediator.Send(new GetTeamExpenseClaimsQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveExpenseClaimCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason)
    {
        var result = await Mediator.Send(new RejectExpenseClaimCommand(id, reason));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPost("{id}/receipt")]
    [Authorize]
    public async Task<IActionResult> UploadReceipt(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("File is required."));

        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadExpenseReceiptCommand(id, stream, file.FileName, file.ContentType));
        
        return result.IsSuccess ? Ok(ApiResponse<string>.Ok(result.Data!)) : BadRequest(ApiResponse<string>.Fail(result.Errors));
    }

    [HttpGet("/api/v1/expense-categories")]
    [Authorize]
    public async Task<IActionResult> GetCategories()
    {
        var result = await Mediator.Send(new GetExpenseCategoriesQuery());
        return result.IsSuccess ? Ok(ApiResponse.Ok(result.Data!)) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
