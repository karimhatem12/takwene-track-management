using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Tests.Domain;

public class EditRulesTests
{
    private static readonly DateOnly NewDate = new(2027, 5, 1);

    [Fact]
    public void A_draft_track_can_change_every_field()
    {
        var track = TestData.DraftTrack();

        track.UpdateDetails(artistId: 2, title: "  New Title ", isrc: "sa-abc-27-00009", NewDate, genre: "Rai");

        Assert.Equal(2, track.ArtistId);
        Assert.Equal("New Title", track.Title);
        Assert.Equal("SAABC2700009", track.Isrc);
        Assert.Equal(NewDate, track.ReleaseDate);
        Assert.Equal("Rai", track.Genre);
    }

    [Fact]
    public void A_submitted_track_can_still_change_title_genre_and_release_date()
    {
        var track = TestData.DraftTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        // Same artist and the same ISRC written differently: allowed.
        track.UpdateDetails(artistId: 1, title: "Renamed", isrc: "egtkw2600001", NewDate, genre: "Rock");

        Assert.Equal("Renamed", track.Title);
        Assert.Equal("Rock", track.Genre);
        Assert.Equal(NewDate, track.ReleaseDate);
        Assert.Equal("EGTKW2600001", track.Isrc);
    }

    [Fact]
    public void A_submitted_track_cannot_change_its_isrc()
    {
        var track = TestData.DraftTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        var ex = Assert.Throws<DomainException>(() =>
            track.UpdateDetails(artistId: 1, title: "Test Track", isrc: "EG-TKW-26-00002", NewDate, genre: "Pop"));

        Assert.Contains("only be changed while the track is a Draft", ex.Message);
        Assert.Equal("EGTKW2600001", track.Isrc);
    }

    [Fact]
    public void A_submitted_track_cannot_move_to_another_artist()
    {
        var track = TestData.DraftTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Throws<DomainException>(() =>
            track.UpdateDetails(artistId: 2, title: "Test Track", isrc: "EG-TKW-26-00001", NewDate, genre: "Pop"));

        Assert.Equal(1, track.ArtistId);
    }

    [Fact]
    public void A_rejected_edit_changes_nothing()
    {
        var track = TestData.DraftTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Throws<DomainException>(() =>
            track.UpdateDetails(artistId: 2, title: "Changed", isrc: "EG-TKW-26-00001", NewDate, genre: "Rock"));

        Assert.Equal("Test Track", track.Title);
        Assert.Equal("Pop", track.Genre);
    }

    [Fact]
    public void Editing_an_artist_normalizes_email_and_country()
    {
        var artist = Artist.Create("Nour", "nour@example.com", "EG");

        artist.Update("  Nour El Sayed ", " Nour.New@Example.COM ", "sa");

        Assert.Equal("Nour El Sayed", artist.Name);
        Assert.Equal("nour.new@example.com", artist.Email);
        Assert.Equal("SA", artist.Country);
    }
}
