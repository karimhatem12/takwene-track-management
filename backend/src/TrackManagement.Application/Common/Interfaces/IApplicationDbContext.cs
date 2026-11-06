using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Common.Interfaces;

/// <summary>
/// The Application layer's view of the database. Implemented in Infrastructure,
/// so Application never depends on SQL Server or on the concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Artist> Artists { get; }
    DbSet<Track> Tracks { get; }
    DbSet<Dsp> Dsps { get; }
    DbSet<TrackDistribution> TrackDistributions { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
