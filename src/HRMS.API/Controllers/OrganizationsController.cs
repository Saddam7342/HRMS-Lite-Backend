using HRMS.Application.Features.Organizations.Commands.CreateOrganization;
using HRMS.Application.Features.Organizations.Commands.Status;
using HRMS.Application.Features.Organizations.Commands.UpdateOrganization;
using HRMS.Application.Features.Organizations.Commands.UploadLogo;
using HRMS.Application.Features.Organizations.DTOs;
using HRMS.Application.Features.Organizations.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class OrganizationsController : BaseApiController
{
    /// <summary>
    /// Creates a new organization (tenant).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
    public async Task<IActionResult> Create(CreateOrganizationCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse<Guid>.Ok(result.Data)) 
            : BadRequest(ApiResponse<Guid>.Fail(result.Errors));
    }

    /// <summary>
    /// Gets all organizations.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetOrganizationsQuery());
        return Ok(ApiResponse<IReadOnlyList<OrganizationDto>>.Ok(result.Data ?? []));
    }

    /// <summary>
    /// Gets organization details by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetOrganizationByIdQuery(id));
        return result.IsSuccess 
            ? Ok(ApiResponse<OrganizationDto>.Ok(result.Data!)) 
            : NotFound(ApiResponse<OrganizationDto>.Fail(result.Errors));
    }

    /// <summary>
    /// Updates organization details.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> Update(Guid id, UpdateOrganizationCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse.Fail("ID mismatch."));
        var result = await Mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse.Ok()) 
            : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Activates an organization.
    /// </summary>
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateOrganizationCommand(id));
        return result.IsSuccess 
            ? Ok(ApiResponse.Ok()) 
            : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Deactivates an organization.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await Mediator.Send(new DeactivateOrganizationCommand(id));
        return result.IsSuccess 
            ? Ok(ApiResponse.Ok()) 
            : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Uploads an organization logo.
    /// </summary>
    [HttpPost("{id}/logo")]
    [Authorize(Roles = "PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse.Fail("No file uploaded."));
        
        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadOrganizationLogoCommand(id, stream, file.FileName, file.ContentType));
        
        return result.IsSuccess 
            ? Ok(ApiResponse<string>.Ok(result.Data!)) 
            : BadRequest(ApiResponse<string>.Fail(result.Errors));
    }

    /// <summary>
    /// Gets branding settings by organization slug.
    /// </summary>
    [HttpGet("branding/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OrganizationBrandingDto>), 200)]
    public async Task<IActionResult> GetBranding(string slug)
    {
        var result = await Mediator.Send(new GetOrganizationBrandingQuery(slug));
        return result.IsSuccess 
            ? Ok(ApiResponse<OrganizationBrandingDto>.Ok(result.Data!)) 
            : NotFound(ApiResponse<OrganizationBrandingDto>.Fail(result.Errors));
    }
}
