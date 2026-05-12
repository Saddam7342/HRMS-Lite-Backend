using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class AuditLog : AuditableEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }

    public AuditActionType ActionType { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
