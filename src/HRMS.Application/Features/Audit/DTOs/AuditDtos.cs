using HRMS.Domain.Enums;

namespace HRMS.Application.Features.Audit.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserName,
    AuditActionType ActionType,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);

public record AuditLogListDto(
    Guid Id,
    string? UserName,
    AuditActionType ActionType,
    string EntityName,
    string EntityId,
    DateTime CreatedAt);

public record EntityAuditHistoryDto(
    string Action,
    string? UserName,
    string? Changes,
    DateTime Timestamp);
