using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;
using TrackManagement.Infrastructure.Persistence.Interceptors;

namespace TrackManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Sample catalogue data, seeded through EF Core migrations (HasData).
/// Artists are fictional; email addresses use the reserved example.com domain.
/// The data follows the domain rules:
///   Draft tracks have no distributions,
///   Submitted tracks have pending/rejected distributions,
///   Distributed tracks are live on at least one DSP.
/// Anonymous objects are used because the entities have private setters.
/// </summary>
internal static class SampleData
{
    // Audit columns for seeded rows: created by "system" (see AuditInterceptor.SystemUser).
    private const string SeededBy = AuditInterceptor.SystemUser;
    private static readonly DateTime SeededAt = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void SeedSampleData(this ModelBuilder modelBuilder)
    {
        SeedDsps(modelBuilder);
        SeedArtists(modelBuilder);
        SeedTracks(modelBuilder);
        SeedDistributions(modelBuilder);
    }

    private static class DspIds
    {
        public const int Spotify = 1;
        public const int AppleMusic = 2;
        public const int YouTube = 3;
        public const int Anghami = 4;
    }

    private static void SeedDsps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dsp>().HasData(
            new { Id = DspIds.Spotify, Name = "Spotify" },
            new { Id = DspIds.AppleMusic, Name = "Apple Music" },
            new { Id = DspIds.YouTube, Name = "YouTube" },
            new { Id = DspIds.Anghami, Name = "Anghami" });
    }

    private static void SeedArtists(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>().HasData(
            new { Id = 1, Name = "Nour El Sayed", Email = "nour.elsayed@example.com", Country = "EG", CreatedAt = SeededAt, CreatedBy = SeededBy },
            new { Id = 2, Name = "Layla Haddad", Email = "layla.haddad@example.com", Country = "LB", CreatedAt = SeededAt, CreatedBy = SeededBy },
            new { Id = 3, Name = "Omar Al-Rashid", Email = "omar.alrashid@example.com", Country = "SA", CreatedAt = SeededAt, CreatedBy = SeededBy },
            new { Id = 4, Name = "Yasmine Benali", Email = "yasmine.benali@example.com", Country = "MA", CreatedAt = SeededAt, CreatedBy = SeededBy });
    }

    private static void SeedTracks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Track>().HasData(
            Track(1, 1, "Leil El Qahera", "EGTKW2400001", new DateOnly(2024, 3, 15), "Arabic Pop", TrackStatus.Distributed),
            Track(2, 1, "Sekket Safar", "EGTKW2400002", new DateOnly(2024, 9, 1), "Mahraganat", TrackStatus.Submitted),
            Track(3, 2, "Beirut Lights", "LBTKW2300001", new DateOnly(2023, 11, 20), "Indie", TrackStatus.Distributed),
            Track(4, 2, "Ya Habibi Taala", "LBTKW2500001", new DateOnly(2025, 2, 14), "R&B", TrackStatus.Draft),
            Track(5, 3, "Najd Nights", "SATKW2400001", new DateOnly(2024, 6, 10), "Khaleeji", TrackStatus.Distributed),
            Track(6, 3, "Riyadh Rhythm", "SATKW2500001", new DateOnly(2025, 5, 5), "Hip Hop", TrackStatus.Submitted),
            Track(7, 3, "Desert Wind", "SATKW2600001", new DateOnly(2026, 12, 1), "Electronic", TrackStatus.Draft),
            Track(8, 4, "Casablanca Blue", "MATKW2400001", new DateOnly(2024, 4, 22), "Rai", TrackStatus.Distributed),
            Track(9, 4, "Atlas", "MATKW2500001", new DateOnly(2025, 8, 30), "Electronic", TrackStatus.Submitted),
            Track(10, 4, "Medina Dreams", "MATKW2600001", new DateOnly(2026, 10, 1), "Arabic Pop", TrackStatus.Draft));
    }

    private static void SeedDistributions(ModelBuilder modelBuilder)
    {
        var id = 0;
        object Dist(int trackId, int dspId, DateTime submittedAt, DistributionStatus status) =>
            new { Id = ++id, TrackId = trackId, DspId = dspId, SubmittedAt = submittedAt, Status = status, CreatedAt = submittedAt, CreatedBy = SeededBy };

        var mar24 = Utc(2024, 3, 1);
        var aug24 = Utc(2024, 8, 20);
        var nov23 = Utc(2023, 11, 5);
        var may24 = Utc(2024, 5, 28);
        var apr25 = Utc(2025, 4, 25);
        var apr24 = Utc(2024, 4, 10);
        var aug25 = Utc(2025, 8, 15);

        modelBuilder.Entity<TrackDistribution>().HasData(
            // 1 Leil El Qahera (Distributed): live everywhere
            Dist(1, DspIds.Spotify, mar24, DistributionStatus.Live),
            Dist(1, DspIds.AppleMusic, mar24, DistributionStatus.Live),
            Dist(1, DspIds.YouTube, mar24, DistributionStatus.Live),
            Dist(1, DspIds.Anghami, mar24, DistributionStatus.Live),
            // 2 Sekket Safar (Submitted): waiting on DSPs
            Dist(2, DspIds.Spotify, aug24, DistributionStatus.Pending),
            Dist(2, DspIds.Anghami, aug24, DistributionStatus.Pending),
            // 3 Beirut Lights (Distributed): one DSP rejected it
            Dist(3, DspIds.Spotify, nov23, DistributionStatus.Live),
            Dist(3, DspIds.AppleMusic, nov23, DistributionStatus.Rejected),
            Dist(3, DspIds.Anghami, nov23, DistributionStatus.Live),
            // 5 Najd Nights (Distributed): one still pending
            Dist(5, DspIds.Spotify, may24, DistributionStatus.Live),
            Dist(5, DspIds.YouTube, may24, DistributionStatus.Live),
            Dist(5, DspIds.Anghami, may24, DistributionStatus.Pending),
            // 6 Riyadh Rhythm (Submitted): rejected + pending
            Dist(6, DspIds.Spotify, apr25, DistributionStatus.Rejected),
            Dist(6, DspIds.AppleMusic, apr25, DistributionStatus.Pending),
            // 8 Casablanca Blue (Distributed)
            Dist(8, DspIds.Spotify, apr24, DistributionStatus.Live),
            Dist(8, DspIds.AppleMusic, apr24, DistributionStatus.Live),
            Dist(8, DspIds.Anghami, apr24, DistributionStatus.Live),
            // 9 Atlas (Submitted)
            Dist(9, DspIds.YouTube, aug25, DistributionStatus.Pending),
            Dist(9, DspIds.Anghami, aug25, DistributionStatus.Pending));
    }

    private static object Track(int id, int artistId, string title, string isrc, DateOnly releaseDate,
        string genre, TrackStatus status) =>
        new { Id = id, ArtistId = artistId, Title = title, Isrc = isrc, ReleaseDate = releaseDate, Genre = genre, Status = status, CreatedAt = SeededAt, CreatedBy = SeededBy };

    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 9, 0, 0, DateTimeKind.Utc);
}
