using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HRMS.Persistence.Interceptors;

public class AuditLogInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    private static readonly string[] ExcludedFields = ["PasswordHash", "RefreshToken", "SecurityStamp", "ConcurrencyStamp"];
    private static readonly string[] AuditableEntities = 
    [
        "Employee", "Department", "LeaveRequest", "ExpenseClaim", 
        "TravelRequest", "AttendanceRecord", "Organization", "AppUser", "Role",
        "Document", "OrganizationSetting", "Payroll", "SalaryStructure"
    ];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = CaptureAuditEntries(context);
        if (auditEntries.Count > 0)
        {
            await context.Set<AuditLog>().AddRangeAsync(auditEntries, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CaptureAuditEntries(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            
            // Basic filtering to avoid logging noise
            if (!AuditableEntities.Contains(entityName)) continue;
            if (entry.Entity is AuditLog) continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                UserId = currentUserService.UserId,
                CreatedAt = dateTimeProvider.UtcNow,
                TenantId = (entry.Entity as ITenantEntity)?.TenantId ?? Guid.Empty,
                ActionType = MapActionType(entry.State)
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (ExcludedFields.Contains(propertyName)) continue;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditLog.EntityId = property.CurrentValue?.ToString() ?? string.Empty;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            if (oldValues.Count > 0) auditLog.OldValues = JsonSerializer.Serialize(oldValues);
            if (newValues.Count > 0) auditLog.NewValues = JsonSerializer.Serialize(newValues);

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    private static AuditActionType MapActionType(EntityState state) => state switch
    {
        EntityState.Added => AuditActionType.Create,
        EntityState.Modified => AuditActionType.Update,
        EntityState.Deleted => AuditActionType.Delete,
        _ => AuditActionType.System
    };
}
