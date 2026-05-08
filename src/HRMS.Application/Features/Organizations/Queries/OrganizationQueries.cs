using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Organizations.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Organizations.Queries;

public record GetOrganizationByIdQuery(Guid Id) : IRequest<Result<OrganizationDto>>;
public record GetOrganizationsQuery : IRequest<Result<IReadOnlyList<OrganizationDto>>>;
public record GetOrganizationBrandingQuery(string Slug) : IRequest<Result<OrganizationBrandingDto>>;

public class OrganizationQueryHandlers(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationDto>>,
      IRequestHandler<GetOrganizationsQuery, Result<IReadOnlyList<OrganizationDto>>>,
      IRequestHandler<GetOrganizationBrandingQuery, Result<OrganizationBrandingDto>>
{
    public async Task<Result<OrganizationDto>> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetByIdAsync(request.Id, cancellationToken);
        if (organization == null) return Result<OrganizationDto>.Failure("Organization not found.");

        return Result<OrganizationDto>.Success(mapper.Map<OrganizationDto>(organization));
    }

    public async Task<Result<IReadOnlyList<OrganizationDto>>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var organizations = await unitOfWork.Organizations.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<OrganizationDto>>.Success(mapper.Map<IReadOnlyList<OrganizationDto>>(organizations));
    }

    public async Task<Result<OrganizationBrandingDto>> Handle(GetOrganizationBrandingQuery request, CancellationToken cancellationToken)
    {
        var organization = await unitOfWork.Organizations.GetBySlugAsync(request.Slug, cancellationToken);
        if (organization == null) return Result<OrganizationBrandingDto>.Failure("Organization not found.");

        return Result<OrganizationBrandingDto>.Success(mapper.Map<OrganizationBrandingDto>(organization));
    }
}
