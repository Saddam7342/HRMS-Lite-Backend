using AutoMapper;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Settings.DTOs;
using HRMS.Domain.Entities;
using HRMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Features.Settings.Commands;

public record UpdateOrganizationSettingCommand(string Key, string Value) : IRequest<Result>;

public record BulkUpdateSettingsCommand(List<UpdateSettingRequest> Settings) : IRequest<Result>;

public class SettingCommandHandlers(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider) 
    : IRequestHandler<UpdateOrganizationSettingCommand, Result>,
      IRequestHandler<BulkUpdateSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateOrganizationSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.Settings.GetByKeyAsync(tenantContext.TenantId, request.Key, cancellationToken);
        if (setting == null) return Result.Failure("Setting not found.");
        if (!setting.IsEditable) return Result.Failure("This setting is read-only.");

        setting.Value = request.Value;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(BulkUpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var keys = request.Settings.Select(x => x.Key).ToList();
        var existingSettings = await unitOfWork.Settings.GetQueryable()
            .Where(x => x.TenantId == tenantContext.TenantId && keys.Contains(x.Key))
            .ToListAsync(cancellationToken);

        foreach (var update in request.Settings)
        {
            var setting = existingSettings.FirstOrDefault(x => x.Key == update.Key);
            if (setting != null && setting.IsEditable)
            {
                setting.Value = update.Value;
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
