using AutoMapper;
using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Organizations.DTOs;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public int MaxEmployeeSlots { get; init; }
}

public class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .MustAsync(BeUniqueName).WithMessage("Organization name already exists.");

        RuleFor(v => v.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(50).WithMessage("Slug must not exceed 50 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.")
            .MustAsync(BeUniqueSlug).WithMessage("Slug already exists.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(v => v.MaxEmployeeSlots)
            .GreaterThan(0).WithMessage("Max employee slots must be greater than 0.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        return !await _unitOfWork.Organizations.ExistsByNameAsync(name, ct);
    }

    private async Task<bool> BeUniqueSlug(string slug, CancellationToken ct)
    {
        return !await _unitOfWork.Organizations.ExistsBySlugAsync(slug, ct);
    }
}

public class CreateOrganizationHandler(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<CreateOrganizationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = new Organization
        {
            Name = request.Name,
            Slug = request.Slug,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            MaxEmployeeSlots = request.MaxEmployeeSlots,
            IsActive = true,
            TenantId = Guid.NewGuid() // The new organization is its own tenant root
        };

        await unitOfWork.Organizations.AddAsync(organization, cancellationToken);
        
        // Seed Default Leave Types
        var leaveTypes = new List<LeaveType>
        {
            new() { Name = "Casual Leave", Code = "CL", DefaultDays = 12, TenantId = organization.Id },
            new() { Name = "Sick Leave", Code = "SL", DefaultDays = 10, TenantId = organization.Id },
            new() { Name = "Annual Leave", Code = "AL", DefaultDays = 15, TenantId = organization.Id },
            new() { Name = "Maternity Leave", Code = "ML", DefaultDays = 90, IsGenderSpecific = true, ApplicableGender = Gender.Female, TenantId = organization.Id },
            new() { Name = "Paternity Leave", Code = "PL", DefaultDays = 7, IsGenderSpecific = true, ApplicableGender = Gender.Male, TenantId = organization.Id }
        };
        await unitOfWork.LeaveTypes.AddRangeAsync(leaveTypes, cancellationToken);

        // Seed Default Expense Categories
        var categories = new List<ExpenseCategory>
        {
            new() { Name = "Travel", Code = "TRAVEL", Description = "Business travel expenses", TenantId = organization.Id },
            new() { Name = "Food", Code = "FOOD", Description = "Meal reimbursements", TenantId = organization.Id },
            new() { Name = "Fuel", Code = "FUEL", Description = "Vehicle fuel", TenantId = organization.Id },
            new() { Name = "Accommodation", Code = "STAY", Description = "Hotel/lodging", TenantId = organization.Id },
            new() { Name = "Office Supplies", Code = "SUPPLY", Description = "Stationery and equipment", TenantId = organization.Id },
            new() { Name = "Medical", Code = "MED", Description = "Health-related claims", TenantId = organization.Id },
            new() { Name = "Other", Code = "OTHER", Description = "Miscellaneous", TenantId = organization.Id }
        };
        await unitOfWork.ExpenseCategories.AddRangeAsync(categories, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(organization.Id);
    }
}
