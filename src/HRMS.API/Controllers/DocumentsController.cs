using HRMS.Application.Features.Documents.Commands;
using HRMS.Application.Features.Documents.DTOs;
using HRMS.Application.Features.Documents.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

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
        return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data)) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetDocumentByIdQuery(id));
        return result.IsSuccess ? Ok(ApiResponse<DocumentDto>.Ok(result.Data!)) : NotFound(ApiResponse.Fail(result.Errors));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var result = await Mediator.Send(new GetEmployeeDocumentsQuery(employeeId));
        return Ok(ApiResponse<IReadOnlyList<DocumentDto>>.Ok(result.Data!));
    }

    [HttpGet("company")]
    public async Task<IActionResult> GetCompanyDocuments()
    {
        var result = await Mediator.Send(new GetCompanyDocumentsQuery());
        return Ok(ApiResponse<IReadOnlyList<DocumentDto>>.Ok(result.Data!));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await Mediator.Send(new DownloadDocumentQuery(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Errors));

        var download = result.Data!;
        return File(download.FileStream, download.ContentType, download.FileName);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var result = await Mediator.Send(new UpdateDocumentCommand(id, request.Title, request.Description, request.Category));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDocumentCommand(id));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    [HttpPost("{id}/version")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadNewVersion(Guid id, IFormFile file)
    {
        var result = await Mediator.Send(new UploadNewVersionCommand(id, file));
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }
}
