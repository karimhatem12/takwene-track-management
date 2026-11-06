using TrackManagement.Domain.Enums;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Domain.Entities;

public class Track : AuditableEntity
{
    private readonly List<TrackDistribution> _distributions = new();

    public int Id { get; private set; }
    public string Title { get; private set; } = null!;
    public int ArtistId { get; private set; }
    public Artist Artist { get; private set; } = null!;

    /// <summary>
    /// International Standard Recording Code, stored as 12 upper-case characters
    /// without hyphens (e.g. "EGA012400001"). Unique across tracks.
    /// </summary>
    public string Isrc { get; private set; } = null!;

    public DateOnly ReleaseDate { get; private set; }
    public string Genre { get; private set; } = null!;
    public TrackStatus Status { get; private set; }

    public IReadOnlyCollection<TrackDistribution> Distributions => _distributions.AsReadOnly();

    // Required by EF Core.
    private Track()
    {
    }

    public static Track Create(int artistId, string title, string isrc, DateOnly releaseDate, string genre)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Track title is required.");
        if (string.IsNullOrWhiteSpace(isrc))
            throw new DomainException("ISRC is required.");
        if (string.IsNullOrWhiteSpace(genre))
            throw new DomainException("Genre is required.");

        return new Track
        {
            ArtistId = artistId,
            Title = title.Trim(),
            Isrc = NormalizeIsrc(isrc),
            ReleaseDate = releaseDate,
            Genre = genre.Trim(),
            Status = TrackStatus.Draft
        };
    }

    /// <summary>
    /// "EG-A01-24-00001" / "ega012400001" -> "EGA012400001".
    /// Format checking is done by the Application validator; this only normalizes.
    /// </summary>
    public static string NormalizeIsrc(string isrc) =>
        isrc.Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();

    /// <summary>
    /// Submits the track to one or more DSPs. Each DSP gets a Pending distribution.
    /// A track can be sent to each DSP only once. A Draft track becomes Submitted.
    /// Callers must load <see cref="Distributions"/> first so duplicates can be detected.
    /// </summary>
    public void Distribute(IEnumerable<Dsp> dsps, DateTime submittedAtUtc)
    {
        var requested = dsps.DistinctBy(d => d.Id).ToList();

        if (requested.Count == 0)
            throw new DomainException("At least one DSP is required to distribute a track.");

        var alreadySent = requested
            .Where(dsp => _distributions.Any(d => d.DspId == dsp.Id))
            .Select(dsp => dsp.Name)
            .ToList();

        if (alreadySent.Count > 0)
            throw new DomainException(
                $"Track was already distributed to: {string.Join(", ", alreadySent)}.");

        foreach (var dsp in requested)
            _distributions.Add(new TrackDistribution(this, dsp, submittedAtUtc));

        if (Status == TrackStatus.Draft)
            Status = TrackStatus.Submitted;
    }

    /// <summary>
    /// Moves the track one step forward: Draft -> Submitted -> Distributed.
    /// Skipping a step or going backwards is not allowed.
    /// Distributed additionally requires at least one DSP to be Live.
    /// </summary>
    public void ChangeStatus(TrackStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new DomainException($"Unknown track status '{newStatus}'.");

        if (newStatus == Status)
            throw new DomainException($"Track is already {Status}.");

        if (newStatus != Status + 1)
            throw new DomainException(
                $"Cannot change status from {Status} to {newStatus}. Status can only move forward one step at a time.");

        if (newStatus == TrackStatus.Distributed &&
            !_distributions.Any(d => d.Status == DistributionStatus.Live))
            throw new DomainException(
                "Track cannot be marked as Distributed until it is live on at least one DSP.");

        Status = newStatus;
    }
}
