using System.Text.Json;
using TrackManagement.Domain.Enums;

namespace TrackManagement.Application.Audit;

/// <summary>
/// One audit entry. <c>Changes</c> holds the changed fields as JSON:
/// { "Status": { "old": "Draft", "new": "Submitted" } }.
/// </summary>
public record AuditLogDto(
    long Id,
    DateTime Timestamp,
    int? UserId,
    string UserEmail,
    AuditAction Action,
    string EntityName,
    string EntityId,
    JsonElement? Changes);

/// <summary>Filters for GET /api/audit-logs. All optional.</summary>
public record AuditLogQuery(
    string? EntityName,
    string? EntityId,
    int? UserId,
    AuditAction? Action,
    int Page = 1,
    int PageSize = 20);
