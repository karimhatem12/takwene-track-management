using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("Tracks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Isrc).HasMaxLength(12).IsFixedLength().IsRequired();
        builder.Property(t => t.Genre).HasMaxLength(50).IsRequired();
        builder.Property(t => t.ReleaseDate).HasColumnType("date");

        // Stored as text ("Draft", "Submitted"...) so the database is readable.
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(t => t.Isrc).IsUnique();
        // Columns used by the GET /api/tracks filters (ArtistId is indexed as a foreign key).
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Genre);

        // EF writes through the private _distributions field behind the read-only property.
        builder.HasMany(t => t.Distributions)
            .WithOne(d => d.Track)
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Distributions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
