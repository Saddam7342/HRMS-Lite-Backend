using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Employees.Commands.UploadImage;

public record UploadEmployeeProfileImageCommand(Guid Id, Stream FileStream, string FileName, string ContentType) : IRequest<Result<string>>;

public class UploadEmployeeProfileImageHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICurrentUserService currentUserService) : IRequestHandler<UploadEmployeeProfileImageCommand, Result<string>>
{
    private const string FolderPath = "employees/profile-images";

    public async Task<Result<string>> Handle(UploadEmployeeProfileImageCommand request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(request.Id, cancellationToken);
        if (employee == null) return Result<string>.Failure("Employee not found.");

        // Security: Self or Admin
        if (currentUserService.UserId != employee.UserId && 
            !currentUserService.Roles.Contains("OrganizationAdmin") && 
            !currentUserService.Roles.Contains("PlatformAdmin"))
        {
            return Result<string>.Failure("Unauthorized.");
        }

        // Delete old image
        if (!string.IsNullOrEmpty(employee.ProfileImageUrl))
        {
            await fileStorageService.DeleteAsync(employee.ProfileImageUrl, cancellationToken);
        }

        // Upload new image
        var fileName = $"{employee.EmployeeCode}-{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";
        var fullPath = $"{FolderPath}/{fileName}";
        
        var imageUrl = await fileStorageService.UploadAsync(request.FileStream, fullPath, request.ContentType, cancellationToken);

        employee.ProfileImageUrl = imageUrl;
        employee.User.ProfileImageUrl = imageUrl;

        unitOfWork.Employees.Update(employee);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<string>.Success(imageUrl);
    }
}
