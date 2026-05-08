using HRMS.Application.Common.Interfaces;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Organizations.Commands.Status;

public record ActivateOrganizationCommand(Guid Id) : IRequest<Result>;
public record DeactivateOrganizationCommand(Guid Id) : IRequest<Result>;

public class OrganizationStatusHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<ActivateOrganizationCommand, Result>,
      IRequestHandler<DeactivateOrganizationCommand, Result>
{
    public async Task<Result> Handle(ActivateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetByIdAsync(request.Id, cancellationToken);
        if (organization == null) return Result.Failure("Organization not found.");

        organization.IsActive = true;
        unitOfWork.Organizations.Update(organization);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(DeactivateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetByIdAsync(request.Id, cancellationToken);
        if (organization == null) return Result.Failure("Organization not found.");

        organization.IsActive = false;
        unitOfWork.Organizations.Update(organization);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
