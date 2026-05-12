using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class Notification : AuditableEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }

    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}
