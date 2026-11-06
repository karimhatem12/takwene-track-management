using TrackManagement.Domain.Entities;

namespace TrackManagement.Tests;

/// <summary>Small builders shared by the tests.</summary>
internal static class TestData
{
    /// <summary>
    /// DSP ids are normally assigned by the database. The domain compares DSPs by id,
    /// so tests set it explicitly through the private setter.
    /// </summary>
    public static Dsp Dsp(int id, string name)
    {
        var dsp = new Dsp(name);
        typeof(Dsp).GetProperty(nameof(global::TrackManagement.Domain.Entities.Dsp.Id))!.SetValue(dsp, id);
        return dsp;
    }

    public static readonly Dsp Spotify = Dsp(1, "Spotify");
    public static readonly Dsp AppleMusic = Dsp(2, "Apple Music");
    public static readonly Dsp Anghami = Dsp(4, "Anghami");

    public static Track DraftTrack() =>
        Track.Create(artistId: 1, title: "Test Track", isrc: "EG-TKW-26-00001",
            releaseDate: new DateOnly(2026, 1, 1), genre: "Pop");

    public static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);
}
