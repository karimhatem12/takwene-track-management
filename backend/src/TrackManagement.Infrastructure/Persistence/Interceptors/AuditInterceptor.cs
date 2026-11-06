using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;

namespace TrackManagement.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Runs on every SaveChanges and
///  1. fills CreatedAt/CreatedBy/UpdatedAt/UpdatedBy on <see cref="AuditableEntity"/> rows, and
///  2. writes one <see cref="AuditLog"/> row per created/updated/deleted entity with the changed fields.
/// Services never write audit rows for data changes themselves, so a change can't be forgotten.
/// The data change and its audit rows are saved in one transaction: both succeed or neither does.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    /// <summary>Recorded as the author when there is no signed-in user (seeding, startup tasks).</summary>
    public const string SystemUser = "system";

    // Bookkeeping columns: already visible on the row itself, so not repeated in the change list.
    private static readonly HashSet<string> IgnoredProperties =
    [
        nameof(AuditableEntity.CreatedAt), nameof(AuditableEntity.CreatedBy),
        nameof(AuditableEntity.UpdatedAt), nameof(AuditableEntity.UpdatedBy)
    ];

    // Never written to the audit log, whatever entity they appear on.
    private static readonly HashSet<string> SensitiveProperties = ["PasswordHash"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    private List<PendingAudit> _pending = [];
    private IDbContextTransaction? _ownTransaction;

    public AuditInterceptor(ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context && Prepare(context) && context.Database.CurrentTransaction is null)
            _ownTransaction = await context.Database.BeginTransactionAsync(cancellationToken);

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context && AddAuditRows(context))
        {
            await context.SaveChangesAsync(cancellationToken);
            if (_ownTransaction is not null)
            {
                await _ownTransaction.CommitAsync(cancellationToken);
                await DisposeTransactionAsync();
            }
        }

        return result;
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending = [];
        if (_ownTransaction is not null)
        {
            await _ownTransaction.RollbackAsync(cancellationToken);
            await DisposeTransactionAsync();
        }
    }

    // Synchronous SaveChanges() is not used by the application; same behaviour for completeness.
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context && Prepare(context) && context.Database.CurrentTransaction is null)
            _ownTransaction = context.Database.BeginTransaction();
        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is { } context && AddAuditRows(context))
        {
            context.SaveChanges();
            _ownTransaction?.Commit();
            _ownTransaction?.Dispose();
            _ownTransaction = null;
        }
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pending = [];
        _ownTransaction?.Rollback();
        _ownTransaction?.Dispose();
        _ownTransaction = null;
    }

    /// <summary>Stamps auditable columns and captures what changed, before the values are saved.</summary>
    private bool Prepare(DbContext context)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var author = _currentUser.Email ?? SystemUser;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.CreatedBy).CurrentValue = author;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedBy).CurrentValue = author;
            }
        }

        _pending = context.ChangeTracker.Entries()
            .Where(e => IsAudited(e.Entity) && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new PendingAudit(e, ToAction(e.State), CaptureChanges(e), now))
            .Where(p => p.Action != AuditAction.Updated || p.Changes.Count > 0) // skip "updates" that changed nothing
            .ToList();

        return _pending.Count > 0;
    }

    /// <summary>After the save, generated ids are known: add one AuditLog row per captured change.</summary>
    private bool AddAuditRows(DbContext context)
    {
        if (_pending.Count == 0)
            return false;

        var pending = _pending;
        _pending = []; // the nested SaveChanges for the audit rows must not audit itself

        var traceId = Activity.Current?.TraceId.ToString();
        foreach (var item in pending)
        {
            context.Set<AuditLog>().Add(AuditLog.Create(
                _currentUser.UserId,
                _currentUser.Email ?? SystemUser,
                item.Action,
                item.Entry.Metadata.ClrType.Name,
                GetKey(item.Entry),
                JsonSerializer.Serialize(item.Changes, JsonOptions),
                item.Timestamp,
                traceId));
        }

        return true;
    }

    private static Dictionary<string, FieldChange> CaptureChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, FieldChange>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (property.Metadata.IsPrimaryKey() || IgnoredProperties.Contains(name))
                continue;

            if (SensitiveProperties.Contains(name))
            {
                if (entry.State == EntityState.Added || property.IsModified)
                    changes[name] = new FieldChange("***", "***");
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changes[name] = new FieldChange(null, property.CurrentValue);
                    break;
                case EntityState.Deleted:
                    changes[name] = new FieldChange(property.OriginalValue, null);
                    break;
                case EntityState.Modified when property.IsModified && !Equals(property.OriginalValue, property.CurrentValue):
                    changes[name] = new FieldChange(property.OriginalValue, property.CurrentValue);
                    break;
            }
        }

        return changes;
    }

    private static string GetKey(EntityEntry entry) =>
        string.Join(",", entry.Metadata.FindPrimaryKey()!.Properties.Select(p => entry.Property(p.Name).CurrentValue));

    // Users are audited through explicit Registered / LoggedIn events (see AuthService),
    // and audit rows never audit themselves.
    private static bool IsAudited(object entity) => entity is not AuditLog and not User;

    private static AuditAction ToAction(EntityState state) => state switch
    {
        EntityState.Added => AuditAction.Created,
        EntityState.Deleted => AuditAction.Deleted,
        _ => AuditAction.Updated
    };

    private async Task DisposeTransactionAsync()
    {
        if (_ownTransaction is not null)
            await _ownTransaction.DisposeAsync();
        _ownTransaction = null;
    }

    private sealed record PendingAudit(
        EntityEntry Entry, AuditAction Action, Dictionary<string, FieldChange> Changes, DateTime Timestamp);

    private sealed record FieldChange(
        [property: JsonPropertyName("old")] object? Old,
        [property: JsonPropertyName("new")] object? New);
}
