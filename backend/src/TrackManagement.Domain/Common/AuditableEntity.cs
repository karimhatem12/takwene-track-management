namespace TrackManagement.Domain.Common;

/// <summary>
/// Base class for entities that record who created and last updated them.
/// The values are filled in automatically by the persistence layer (AuditInterceptor)
/// when the entity is saved; domain code never sets them.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; private set; }

    /// <summary>Email of the user who created the row, or "system" for seed data.</summary>
    public string CreatedBy { get; private set; } = null!;

    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
}
