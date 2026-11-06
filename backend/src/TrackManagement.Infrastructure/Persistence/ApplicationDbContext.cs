using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Entities;
using TrackManagement.Infrastructure.Persistence.Converters;
using TrackManagement.Infrastructure.Persistence.Seed;

namespace TrackManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    // SQL Server error numbers for "duplicate key" on a unique index / unique constraint.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Dsp> Dsps => Set<Dsp>();
    public DbSet<TrackDistribution> TrackDistributions => Set<TrackDistribution>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Every DateTime in the model is UTC (see UtcDateTimeConverter).
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in Persistence/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Same column setup for the audit columns on every AuditableEntity (Artist, Track, TrackDistribution).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.Property(nameof(AuditableEntity.CreatedAt)).HasColumnType("datetime2");
            entity.Property(nameof(AuditableEntity.CreatedBy)).HasMaxLength(256).IsRequired();
            entity.Property(nameof(AuditableEntity.UpdatedAt)).HasColumnType("datetime2");
            entity.Property(nameof(AuditableEntity.UpdatedBy)).HasMaxLength(256);
        }

        // Sample artists, tracks, DSPs and distributions (see Persistence/Seed/SampleData.cs).
        modelBuilder.SeedSampleData();
    }

    /// <summary>
    /// Services check for duplicates before saving, but two requests at the same moment
    /// can both pass that check. The unique index then rejects the second insert;
    /// we turn that SQL error into a ConflictException (409) instead of a 500.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
                                           {
                                               Number: UniqueIndexViolation or UniqueConstraintViolation
                                           })
        {
            throw new ConflictException("The record conflicts with existing data (duplicate value).");
        }
    }
}
