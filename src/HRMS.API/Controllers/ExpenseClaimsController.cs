using Asp.Versioning;
using HRMS.API.Models;
using HRMS.Application.Features.Expenses.Commands.Approval;
using HRMS.Application.Features.Expenses.Commands.Create;
using HRMS.Application.Features.Expenses.Commands.Receipt;
using HRMS.Application.Features.Expenses.DTOs;
using HRMS.Application.Features.Expenses.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class ExpenseClaimsController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateExpenseClaimCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyClaims()
    {
        var result = await Mediator.Send(new GetMyExpenseClaimsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var result = await Mediator.Send(new GetPendingExpenseApprovalsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("team")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTeamClaims()
    {
        var result = await Mediator.Send(new GetTeamExpenseClaimsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("categories")]
    [HttpGet("~/api/v1/expense-categories")]
    [Authorize]
    public async Task<IActionResult> GetCategories()
    {
        var result = await Mediator.Send(new GetExpenseCategoriesQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetExpenseClaimByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveExpenseClaimCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReasonRequest? body)
    {
        var result = await Mediator.Send(new RejectExpenseClaimCommand(id, body?.Reason));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPost("{id:guid}/receipt")]
    [Authorize]
    public async Task<IActionResult> UploadReceipt(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadEnvelope("File is required.");

        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadExpenseReceiptCommand(id, stream, file.FileName, file.ContentType));

        return result.IsSuccess ? OkData(result) : BadData(result);
    }
}
