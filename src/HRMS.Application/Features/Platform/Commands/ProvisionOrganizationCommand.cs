using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Platform.Commands;

public record ProvisionOrganizationCommand(
    string Name, 
    string Slug, 
    string AdminEmail, 
    int MaxEmployeeSlots = 50) : IRequest<Result<Guid>>;

public class ProvisionOrganizationValidator : AbstractValidator<ProvisionOrganizationCommand>
{
    public ProvisionOrganizationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.MaxEmployeeSlots).GreaterThan(0);
    }
}

public class ProvisionOrganizationHandler(IOrganizationProvisioningService provisioningService) 
    : IRequestHandler<ProvisionOrganizationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ProvisionOrganizationCommand request, CancellationToken cancellationToken)
    {
        return await provisioningService.ProvisionOrganizationAsync(
            request.Name, 
            request.Slug, 
            request.AdminEmail, 
            request.MaxEmployeeSlots, 
            cancellationToken);
    }
}
