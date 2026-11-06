using TrackManagement.Domain.Entities;
using TrackManagement.Domain.Enums;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Tests.Domain;

public class TrackDistributionTests
{
    [Fact]
    public void Distribute_creates_one_pending_distribution_per_dsp()
    {
        var track = TestData.DraftTrack();

        track.Distribute([TestData.Spotify, TestData.Anghami], TestData.Now);

        Assert.Equal(2, track.Distributions.Count);
        Assert.All(track.Distributions, d =>
        {
            Assert.Equal(DistributionStatus.Pending, d.Status);
            Assert.Equal(TestData.Now, d.SubmittedAt);
        });
    }

    [Fact]
    public void Distributing_a_draft_track_moves_it_to_submitted()
    {
        var track = TestData.DraftTrack();

        track.Distribute([TestData.Spotify], TestData.Now);

        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Distribute_requires_at_least_one_dsp()
    {
        var track = TestData.DraftTrack();

        Assert.Throws<DomainException>(() => track.Distribute([], TestData.Now));
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void Same_dsp_twice_in_one_request_is_counted_once()
    {
        var track = TestData.DraftTrack();

        track.Distribute([TestData.Spotify, TestData.Spotify], TestData.Now);

        Assert.Single(track.Distributions);
    }

    [Fact]
    public void A_track_cannot_be_sent_to_the_same_dsp_twice()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify], TestData.Now);

        var ex = Assert.Throws<DomainException>(() =>
            track.Distribute([TestData.AppleMusic, TestData.Spotify], TestData.Now));

        Assert.Contains("Spotify", ex.Message);
        // Nothing from the rejected request is added, not even Apple Music.
        Assert.Single(track.Distributions);
    }

    [Fact]
    public void A_submitted_track_can_still_be_sent_to_new_dsps()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify], TestData.Now);

        track.Distribute([TestData.Anghami], TestData.Now);

        Assert.Equal(2, track.Distributions.Count);
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void A_distribution_outcome_is_final()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify], TestData.Now);
        var distribution = track.Distributions.Single();

        distribution.MarkLive();

        Assert.Throws<DomainException>(() => distribution.Reject());
        Assert.Equal(DistributionStatus.Live, distribution.Status);
    }

    [Theory]
    [InlineData("EG-TKW-24-00001", "EGTKW2400001")]
    [InlineData("egtkw2400001", "EGTKW2400001")]
    [InlineData("EG TKW 24 00001", "EGTKW2400001")]
    public void Isrc_is_stored_without_hyphens_in_upper_case(string input, string expected)
    {
        Assert.Equal(expected, Track.NormalizeIsrc(input));
    }
}
