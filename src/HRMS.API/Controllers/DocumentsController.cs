using Asp.Versioning;
using HRMS.Application.Features.Documents.Commands;
using HRMS.Application.Features.Documents.DTOs;
using HRMS.Application.Features.Documents.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class DocumentsController : BaseApiController
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
    {
        var command = new UploadDocumentCommand(
            request.Title,
            request.Description,
            request.DocumentType,
            request.Category,
            request.EmployeeId,
            request.File);

        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("company")]
    public async Task<IActionResult> GetCompanyDocuments()
    {
        var result = await Mediator.Send(new GetCompanyDocumentsQuery());
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var result = await Mediator.Send(new GetEmployeeDocumentsQuery(employeeId));
        return result.IsSuccess ? OkData(result) : BadData(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await Mediator.Send(new DownloadDocumentQuery(id));
        if (!result.IsSuccess)
            return BadData(result);

        var download = result.Data!;
        return File(download.FileStream, download.ContentType, download.FileName);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetDocumentByIdQuery(id));
        return result.IsSuccess ? OkData(result) : NotFoundData(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var result = await Mediator.Send(new UpdateDocumentCommand(id, request.Title, request.Description, request.Category));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDocumentCommand(id));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    [HttpPost("{id:guid}/version")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadNewVersion(Guid id, IFormFile file)
    {
        var result = await Mediator.Send(new UploadNewVersionCommand(id, file));
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }
}
