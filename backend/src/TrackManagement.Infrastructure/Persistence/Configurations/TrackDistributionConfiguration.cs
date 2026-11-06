using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

public class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.ToTable("TrackDistributions");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.SubmittedAt).HasColumnType("datetime2");
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        // A track can be sent to each DSP only once. The domain checks this first;
        // the unique index is the safety net (e.g. two requests at the same time).
        builder.HasIndex(d => new { d.TrackId, d.DspId }).IsUnique();

        // A DSP that still has distributions cannot be deleted.
        builder.HasOne(d => d.Dsp)
            .WithMany()
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
