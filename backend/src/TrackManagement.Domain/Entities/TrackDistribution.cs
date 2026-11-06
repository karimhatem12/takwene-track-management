using TrackManagement.Domain.Enums;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Domain.Entities;

/// <summary>
/// One submission of a track to one DSP. Created only through <see cref="Track.Distribute"/>.
/// </summary>
public class TrackDistribution : AuditableEntity
{
    public int Id { get; private set; }
    public int TrackId { get; private set; }
    public Track Track { get; private set; } = null!;
    public int DspId { get; private set; }
    public Dsp Dsp { get; private set; } = null!;
    public DateTime SubmittedAt { get; private set; }
    public DistributionStatus Status { get; private set; }

    // Required by EF Core.
    private TrackDistribution()
    {
    }

    internal TrackDistribution(Track track, Dsp dsp, DateTime submittedAtUtc)
    {
        Track = track;
        TrackId = track.Id;
        Dsp = dsp;
        DspId = dsp.Id;
        SubmittedAt = submittedAtUtc;
        Status = DistributionStatus.Pending;
    }

    /// <summary>The DSP accepted the track and it is now available to listeners.</summary>
    public void MarkLive()
    {
        EnsurePending();
        Status = DistributionStatus.Live;
    }

    /// <summary>The DSP refused the track (e.g. metadata or rights issue).</summary>
    public void Reject()
    {
        EnsurePending();
        Status = DistributionStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != DistributionStatus.Pending)
            throw new DomainException($"Distribution is already {Status} and can no longer change.");
    }
}
