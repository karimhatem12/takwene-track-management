using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;
using TrackManagement.Infrastructure.Persistence;
using TrackManagement.Infrastructure.Persistence.Interceptors;

namespace TrackManagement.Tests.Audit;

/// <summary>
/// Runs the real AuditInterceptor against EF Core's in-memory provider
/// (fast, no SQL Server needed) to check what ends up in the audit trail.
/// </summary>
public class AuditInterceptorTests
{
    private sealed class FakeCurrentUser(int? userId, string? email) : ICurrentUserService
    {
        public int? UserId { get; } = userId;
        public string? Email { get; } = email;
    }

    private sealed class FixedTime(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }

    private static ApplicationDbContext CreateContext(ICurrentUserService user)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh database per test
            .AddInterceptors(new AuditInterceptor(user, new FixedTime(TestData.Now)))
            // The in-memory provider has no transactions; the interceptor's transaction is a no-op here.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated(); // loads the seed data (artists, tracks, DSPs)
        return db;
    }

    [Fact]
    public async Task Creating_a_track_records_who_created_it_and_its_values()
    {
        await using var db = CreateContext(new FakeCurrentUser(7, "operator@takwene.local"));

        var track = TestData.DraftTrack();
        db.Tracks.Add(track);
        await db.SaveChangesAsync();

        // Audit columns on the row itself.
        Assert.Equal("operator@takwene.local", track.CreatedBy);
        Assert.Equal(TestData.Now, track.CreatedAt);
        Assert.Null(track.UpdatedBy);

        // One audit entry, pointing at the generated id.
        var log = Assert.Single(db.AuditLogs);
        Assert.Equal(AuditAction.Created, log.Action);
        Assert.Equal(nameof(Track), log.EntityName);
        Assert.Equal(track.Id.ToString(), log.EntityId);
        Assert.Equal(7, log.UserId);

        using var changes = JsonDocument.Parse(log.Changes!);
        Assert.Equal("Test Track", changes.RootElement.GetProperty("Title").GetProperty("new").GetString());
        Assert.Equal("Draft", changes.RootElement.GetProperty("Status").GetProperty("new").GetString());
        Assert.False(changes.RootElement.TryGetProperty("CreatedBy", out _)); // bookkeeping columns are not repeated
    }

    [Fact]
    public async Task Updating_a_track_records_only_the_changed_field_with_old_and_new_value()
    {
        await using var db = CreateContext(new FakeCurrentUser(7, "operator@takwene.local"));
        var track = await db.Tracks.SingleAsync(t => t.Id == 4); // seeded Draft track

        track.ChangeStatus(TrackStatus.Submitted);
        await db.SaveChangesAsync();

        Assert.Equal("operator@takwene.local", track.UpdatedBy);

        var log = Assert.Single(db.AuditLogs);
        Assert.Equal(AuditAction.Updated, log.Action);
        Assert.Equal("4", log.EntityId);
        Assert.Equal("""{"Status":{"old":"Draft","new":"Submitted"}}""", log.Changes);
    }

    [Fact]
    public async Task Changes_without_a_signed_in_user_are_recorded_as_system()
    {
        await using var db = CreateContext(new FakeCurrentUser(null, null));

        db.Tracks.Add(TestData.DraftTrack());
        await db.SaveChangesAsync();

        var log = Assert.Single(db.AuditLogs);
        Assert.Null(log.UserId);
        Assert.Equal(AuditInterceptor.SystemUser, log.UserEmail);
    }

    [Fact]
    public async Task Users_and_password_hashes_never_reach_the_audit_log()
    {
        await using var db = CreateContext(new FakeCurrentUser(null, null));

        db.Users.Add(User.Create("New User", "new@example.com", "AQAAAAIAAYagAAAAEFakeHash", TestData.Now));
        await db.SaveChangesAsync();

        Assert.Empty(db.AuditLogs); // users are audited only through explicit Registered/LoggedIn events
    }
}
