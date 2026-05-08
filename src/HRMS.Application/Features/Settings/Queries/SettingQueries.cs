using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Settings.DTOs;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Settings.Queries;

public record GetAllSettingsQuery : IRequest<Result<IReadOnlyList<OrganizationSettingDto>>>;
public record GetSettingsByModuleQuery(string Module) : IRequest<Result<IReadOnlyList<OrganizationSettingDto>>>;

public class SettingQueryHandlers(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IMapper mapper) 
    : IRequestHandler<GetAllSettingsQuery, Result<IReadOnlyList<OrganizationSettingDto>>>,
      IRequestHandler<GetSettingsByModuleQuery, Result<IReadOnlyList<OrganizationSettingDto>>>
{
    public async Task<Result<IReadOnlyList<OrganizationSettingDto>>> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await unitOfWork.Settings.GetByTenantAsync(tenantContext.TenantId, cancellationToken);
        return Result<IReadOnlyList<OrganizationSettingDto>>.Success(mapper.Map<IReadOnlyList<OrganizationSettingDto>>(settings));
    }

    public async Task<Result<IReadOnlyList<OrganizationSettingDto>>> Handle(GetSettingsByModuleQuery request, CancellationToken cancellationToken)
    {
        var prefix = request.Module.ToLower() switch
        {
            "leave" => "leave.",
            "attendance" => "attendance.",
            "expense" => "expense.",
            "travel" => "travel.",
            _ => request.Module + "."
        };

        var settings = await unitOfWork.Settings.GetByModuleAsync(tenantContext.TenantId, prefix, cancellationToken);
        return Result<IReadOnlyList<OrganizationSettingDto>>.Success(mapper.Map<IReadOnlyList<OrganizationSettingDto>>(settings));
    }
}
