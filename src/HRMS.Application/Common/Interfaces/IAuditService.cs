using HRMS.Domain.Enums;

namespace HRMS.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogActivityAsync(
        AuditActionType actionType, 
        string entityName, 
        string entityId, 
        string? message = null, 
        object? oldValues = null, 
        object? newValues = null,
        CancellationToken ct = default);
}
