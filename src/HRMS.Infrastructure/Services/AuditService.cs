using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.Services;

public class AuditService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogActivityAsync(
        AuditActionType actionType,
        string entityName,
        string entityId,
        string? message = null,
        object? oldValues = null, 
        object? newValues = null,
        Guid? tenantId = null,
        CancellationToken ct = default)
    {
        var context = httpContextAccessor.HttpContext;
        
        var auditLog = new AuditLog
        {
            UserId = currentUserService.UserId,
            TenantId = tenantId ?? currentUserService.TenantId ?? Guid.Empty,
            ActionType = actionType,
            EntityName = entityName,
            EntityId = entityId,
            CreatedAt = dateTimeProvider.UtcNow,
            IpAddress = context?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = context?.Request?.Headers["User-Agent"].ToString()
        };

        if (oldValues != null) auditLog.OldValues = JsonSerializer.Serialize(oldValues);
        if (newValues != null) auditLog.NewValues = JsonSerializer.Serialize(newValues);

        await unitOfWork.AuditLogs.AddAsync(auditLog, ct);
    }
}
