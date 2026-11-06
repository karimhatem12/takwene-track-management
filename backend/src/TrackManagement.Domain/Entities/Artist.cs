using TrackManagement.Domain.Common;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Domain.Entities;

public class Artist : AuditableEntity
{
    private readonly List<Track> _tracks = new();

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    /// <summary>Stored lower-case; unique across artists.</summary>
    public string Email { get; private set; } = null!;

    /// <summary>ISO 3166-1 alpha-2 country code, stored upper-case (e.g. "EG", "SA").</summary>
    public string Country { get; private set; } = null!;

    public IReadOnlyCollection<Track> Tracks => _tracks.AsReadOnly();

    // Required by EF Core.
    private Artist()
    {
    }

    public static Artist Create(string name, string email, string country)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Artist name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Artist email is required.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Artist country is required.");

        return new Artist
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Country = country.Trim().ToUpperInvariant()
        };
    }
}
