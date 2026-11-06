using TrackManagement.Domain.Enums;

namespace TrackManagement.Domain.Entities;

/// <summary>
/// One entry in the append-only audit trail: who did what to which record, and when.
/// Rows are only ever inserted; nothing in the application updates or deletes them.
/// </summary>
public class AuditLog
{
    public long Id { get; private set; }

    /// <summary>The user who made the change; null for anonymous or system actions.</summary>
    public int? UserId { get; private set; }

    /// <summary>Copied at the time of the action so the log stays readable even if the user changes later.</summary>
    public string UserEmail { get; private set; } = null!;

    public AuditAction Action { get; private set; }

    /// <summary>e.g. "Track", "Artist", "TrackDistribution", "User".</summary>
    public string EntityName { get; private set; } = null!;

    public string EntityId { get; private set; } = null!;

    /// <summary>
    /// JSON of the changed fields: { "Status": { "old": "Draft", "new": "Submitted" } }.
    /// Null for events without field changes (e.g. a login).
    /// </summary>
    public string? Changes { get; private set; }

    public DateTime Timestamp { get; private set; }

    /// <summary>Request trace id, to match an audit entry with the server logs.</summary>
    public string? TraceId { get; private set; }

    // Required by EF Core.
    private AuditLog()
    {
    }

    public static AuditLog Create(
        int? userId,
        string userEmail,
        AuditAction action,
        string entityName,
        string entityId,
        string? changes,
        DateTime timestampUtc,
        string? traceId) =>
        new()
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes,
            Timestamp = timestampUtc,
            TraceId = traceId
        };
}
