namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Abstraction for date/time to make handlers fully testable without
/// touching system clock. Always use this instead of DateTime.UtcNow.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
