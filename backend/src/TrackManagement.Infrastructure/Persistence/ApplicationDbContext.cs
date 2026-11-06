using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;
using TrackManagement.Infrastructure.Persistence.Seed;

namespace TrackManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Dsp> Dsps => Set<Dsp>();
    public DbSet<TrackDistribution> TrackDistributions => Set<TrackDistribution>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in Persistence/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Sample artists, tracks, DSPs and distributions (see Persistence/Seed/SampleData.cs).
        modelBuilder.SeedSampleData();
    }
}
