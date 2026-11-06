using TrackManagement.Application.Tracks;
using TrackManagement.Domain.Common;
using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Tests.Domain;

public class GenreTests
{
    [Theory]
    [InlineData("hip hop", "Hip Hop")]
    [InlineData("  RAI ", "Rai")]
    [InlineData("r&b", "R&B")]
    public void Genre_is_stored_in_its_canonical_spelling(string input, string expected)
    {
        var track = Track.Create(1, "Song", "EG-TKW-26-00001", new DateOnly(2026, 1, 1), input);

        Assert.Equal(expected, track.Genre);
    }

    [Theory]
    [InlineData("HipHop")]   // near-duplicate the fixed list exists to prevent
    [InlineData("Polka")]
    public void Unknown_genre_is_rejected_by_the_domain(string genre)
    {
        Assert.Throws<DomainException>(() =>
            Track.Create(1, "Song", "EG-TKW-26-00001", new DateOnly(2026, 1, 1), genre));
    }

    [Fact]
    public void Editing_to_an_unknown_genre_is_rejected_and_changes_nothing()
    {
        var track = TestData.DraftTrack();

        Assert.Throws<DomainException>(() =>
            track.UpdateDetails(1, "Renamed", "EG-TKW-26-00001", new DateOnly(2026, 1, 1), "Polka"));

        Assert.Equal("Pop", track.Genre);
        Assert.Equal("Test Track", track.Title);
    }

    [Fact]
    public void Validator_rejects_unknown_genre_with_the_allowed_list()
    {
        var result = new CreateTrackRequestValidator().Validate(
            new CreateTrackRequest(1, "Song", "EG-TKW-26-00001", new DateOnly(2026, 1, 1), "Polka"));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateTrackRequest.Genre), error.PropertyName);
        Assert.Contains("Arabic Pop", error.ErrorMessage);
    }

    [Fact]
    public void Every_seeded_genre_is_in_the_list()
    {
        string[] seeded = ["Arabic Pop", "Electronic", "Hip Hop", "Indie", "Khaleeji", "Mahraganat", "R&B", "Rai"];

        Assert.All(seeded, genre => Assert.True(Genres.IsKnown(genre), genre));
    }
}
