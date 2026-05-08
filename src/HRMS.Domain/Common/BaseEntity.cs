using HRMS.Domain.Common.Interfaces;

namespace HRMS.Domain.Common;

/// <summary>
/// Base entity with a Guid primary key.
/// All domain entities should inherit from this or one of its subclasses.
/// </summary>
public abstract class BaseEntity : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
