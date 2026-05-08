using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string? Designation { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? ManagerId { get; init; }
    public string? Address { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty();
    }
}

public class UpdateEmployeeHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.Employees.GetWithUserAndDepartmentAsync(request.Id, cancellationToken);
        if (employee == null) return Result.Failure("Employee not found.");

        // Security Check: Only OrgAdmin can edit other employees. 
        // Employees can only edit their own profile (and maybe even then, restricted).
        var isAdmin = currentUserService.Roles.Contains("PlatformAdmin") || currentUserService.Roles.Contains("OrganizationAdmin");
        var isSelf = currentUserService.UserId == employee.UserId;

        if (!isAdmin && !isSelf)
            return Result.Failure("Unauthorized to update this profile.");

        // Apply changes
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.PhoneNumber = request.PhoneNumber;
        employee.Gender = request.Gender;
        employee.DateOfBirth = request.DateOfBirth;
        employee.Address = request.Address;
        employee.EmergencyContactName = request.EmergencyContactName;
        employee.EmergencyContactPhone = request.EmergencyContactPhone;

        // Core fields (restricted to Admin)
        if (isAdmin)
        {
            employee.Designation = request.Designation;
            employee.DepartmentId = request.DepartmentId;
            employee.ManagerId = request.ManagerId;
            
            // Sync with AppUser
            employee.User.FirstName = request.FirstName;
            employee.User.LastName = request.LastName;
        }

        unitOfWork.Employees.Update(employee);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success("Profile updated successfully.");
    }
}
