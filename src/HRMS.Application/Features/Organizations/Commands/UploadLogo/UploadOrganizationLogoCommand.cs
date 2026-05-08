using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Organizations.Commands.UploadLogo;

public record UploadOrganizationLogoCommand(Guid Id, Stream FileStream, string FileName, string ContentType) : IRequest<Result<string>>;

public class UploadOrganizationLogoHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService) 
    : IRequestHandler<UploadOrganizationLogoCommand, Result<string>>
{
    private const string FolderPath = "organizations/logos";

    public async Task<Result<string>> Handle(UploadOrganizationLogoCommand request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetByIdAsync(request.Id, cancellationToken);
        if (organization == null) return Result<string>.Failure("Organization not found.");

        // Delete old logo if exists
        if (!string.IsNullOrEmpty(organization.LogoUrl))
        {
            await fileStorageService.DeleteAsync(organization.LogoUrl, cancellationToken);
        }

        // Upload new logo
        var fileName = $"{organization.Slug}-logo{Path.GetExtension(request.FileName)}";
        var logoUrl = await fileStorageService.UploadAsync(request.FileStream, fileName, request.ContentType, cancellationToken);

        organization.LogoUrl = logoUrl;
        unitOfWork.Organizations.Update(organization);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<string>.Success(logoUrl);
    }
}
