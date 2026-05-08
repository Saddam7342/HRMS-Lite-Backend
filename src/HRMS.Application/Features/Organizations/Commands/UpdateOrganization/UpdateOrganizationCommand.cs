using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;
using FluentValidation;

namespace HRMS.Application.Features.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public int MaxEmployeeSlots { get; init; }
}

public class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200);

        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(v => v.MaxEmployeeSlots)
            .GreaterThan(0);
            
        RuleFor(v => v.PrimaryColor)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").When(x => !string.IsNullOrEmpty(x.PrimaryColor))
            .WithMessage("Invalid hex color format.");
            
        RuleFor(v => v.SecondaryColor)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").When(x => !string.IsNullOrEmpty(x.SecondaryColor))
            .WithMessage("Invalid hex color format.");
    }
}

public class UpdateOrganizationHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrganizationCommand, Result>
{
    public async Task<Result> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetByIdAsync(request.Id, cancellationToken);
        
        if (organization == null)
            return Result.Failure("Organization not found.");

        organization.Name = request.Name;
        organization.Email = request.Email;
        organization.PhoneNumber = request.PhoneNumber;
        organization.Address = request.Address;
        organization.PrimaryColor = request.PrimaryColor ?? organization.PrimaryColor;
        organization.SecondaryColor = request.SecondaryColor ?? organization.SecondaryColor;
        organization.MaxEmployeeSlots = request.MaxEmployeeSlots;

        unitOfWork.Organizations.Update(organization);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
