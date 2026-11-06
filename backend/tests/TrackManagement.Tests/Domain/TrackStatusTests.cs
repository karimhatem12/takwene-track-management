using TrackManagement.Domain.Enums;
using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Tests.Domain;

public class TrackStatusTests
{
    [Fact]
    public void New_track_starts_as_draft()
    {
        var track = TestData.DraftTrack();

        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void Draft_can_move_to_submitted()
    {
        var track = TestData.DraftTrack();

        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Cannot_skip_from_draft_to_distributed()
    {
        var track = TestData.DraftTrack();

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Distributed));

        Assert.Contains("one step at a time", ex.Message);
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void Cannot_go_backwards()
    {
        var track = TestData.DraftTrack();
        track.ChangeStatus(TrackStatus.Submitted);

        Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Draft));
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Cannot_change_to_the_same_status()
    {
        var track = TestData.DraftTrack();

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Draft));

        Assert.Contains("already", ex.Message);
    }

    [Fact]
    public void Cannot_mark_distributed_while_no_dsp_is_live()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify], TestData.Now); // -> Submitted, Spotify pending

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Distributed));

        Assert.Contains("live on at least one DSP", ex.Message);
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void Can_mark_distributed_once_a_dsp_is_live()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify, TestData.Anghami], TestData.Now);
        track.Distributions.Single(d => d.DspId == TestData.Anghami.Id).MarkLive();

        track.ChangeStatus(TrackStatus.Distributed);

        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Fact]
    public void Rejected_distribution_does_not_count_as_live()
    {
        var track = TestData.DraftTrack();
        track.Distribute([TestData.Spotify], TestData.Now);
        track.Distributions.Single().Reject();

        Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Distributed));
    }
}
