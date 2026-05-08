namespace HRMS.Domain.Common.Interfaces;

/// <summary>
/// Marker interface for all domain entities.
/// Enforces that every entity has a primary key.
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}
