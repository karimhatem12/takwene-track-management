namespace TrackManagement.Domain.Entities;

/// <summary>
/// Digital Service Provider (e.g. Spotify, Apple Music, YouTube, Anghami).
/// Reference data: DSPs are seeded, not created through the API.
/// </summary>
public class Dsp
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    // Required by EF Core.
    private Dsp()
    {
    }

    public Dsp(string name)
    {
        Name = name;
    }
}
