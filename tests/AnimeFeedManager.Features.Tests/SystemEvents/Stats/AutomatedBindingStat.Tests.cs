using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class AutomatedBindingStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 5, 0, 0, TimeSpan.Zero);
    private static readonly Guid LatestRun = Guid.NewGuid();
    private static readonly Guid EarlierRun = Guid.NewGuid();

    #region Run Selection Tests

    [Fact]
    public void Should_Return_No_Run_When_There_Are_No_Run_Events()
    {
        var result = AutomatedBindingStat.Summarize([], [Bound("a", LatestRun)], []);

        Assert.IsType<NoAutomatedBinding>(result);
    }

    [Fact]
    public void Should_Summarize_Latest_Run_When_Several_Runs_Are_In_Period()
    {
        var runs = ImmutableArray.Create(
            Run(EarlierRun, Now.AddDays(-1), queued: 4),
            Run(LatestRun, Now, queued: 2));

        var result = Assert.IsType<LatestAutomatedBinding>(AutomatedBindingStat.Summarize(runs, [], []));

        Assert.Equal(Now, result.At);
        Assert.Equal(2, result.Queued);
    }

    [Fact]
    public void Should_Carry_Run_Ties_And_Skip_When_Summarizing()
    {
        ImmutableArray<ScheduleTie> ties = [new("Show", [new TiedSeries("a", "Title a"), new TiedSeries("b", "Title b")])];
        var runs = ImmutableArray.Create(Run(LatestRun, Now, queued: 0, ties, skipped: true));

        var result = Assert.IsType<LatestAutomatedBinding>(AutomatedBindingStat.Summarize(runs, [], []));

        Assert.Equal(ties, result.Ties);
        Assert.True(result.ScheduleSkipped);
    }

    #endregion

    #region Outcome Tests

    [Fact]
    public void Should_Count_Only_Latest_Run_Outcomes_When_Earlier_Runs_Bound_Series()
    {
        var runs = ImmutableArray.Create(Run(EarlierRun, Now.AddDays(-1), queued: 2), Run(LatestRun, Now, queued: 3));
        var bound = ImmutableArray.Create(Bound("a", EarlierRun), Bound("b", EarlierRun), Bound("c", LatestRun));
        var failed = ImmutableArray.Create(Failed("d", EarlierRun), Failed("e", LatestRun));

        var result = Assert.IsType<LatestAutomatedBinding>(AutomatedBindingStat.Summarize(runs, bound, failed));

        Assert.Equal(1, result.Bound);
        Assert.Equal([new FailedBind("e", "Release e")], result.Failed);
    }

    [Fact]
    public void Should_Ignore_Manual_Binds_When_Counting_Run_Outcomes()
    {
        var runs = ImmutableArray.Create(Run(LatestRun, Now, queued: 1));
        var bound = ImmutableArray.Create(Bound("a", LatestRun), Bound("m", Guid.NewGuid(), BindingOrigin.Manual));

        var result = Assert.IsType<LatestAutomatedBinding>(AutomatedBindingStat.Summarize(runs, bound, []));

        Assert.Equal(1, result.Bound);
    }

    [Fact]
    public void Should_Report_Pending_When_Outcomes_Have_Not_Arrived()
    {
        var runs = ImmutableArray.Create(Run(LatestRun, Now, queued: 5));
        var bound = ImmutableArray.Create(Bound("a", LatestRun), Bound("b", LatestRun));

        var result = Assert.IsType<LatestAutomatedBinding>(
            AutomatedBindingStat.Summarize(runs, bound, [Failed("c", LatestRun)]));

        Assert.Equal(2, result.Pending);
    }

    [Fact]
    public void Should_Not_Report_Negative_Pending_When_Outcomes_Exceed_Queued()
    {
        var runs = ImmutableArray.Create(Run(LatestRun, Now, queued: 1));
        var bound = ImmutableArray.Create(Bound("a", LatestRun), Bound("b", LatestRun));

        var result = Assert.IsType<LatestAutomatedBinding>(AutomatedBindingStat.Summarize(runs, bound, []));

        Assert.Equal(0, result.Pending);
    }

    #endregion

    #region Test Helpers

    private static EventData<AutomatedBindingCompleted> Run(Guid runId, DateTimeOffset at, int queued,
        ImmutableArray<ScheduleTie> ties = default, bool skipped = false) =>
        new(EventType.Completed, at,
            new AutomatedBindingCompleted(runId, Scheduled: 10, queued, ties.IsDefault ? [] : ties, skipped));

    private static EventData<NyaaSeriesBound> Bound(string seriesId, Guid runId,
        string origin = BindingOrigin.Schedule) =>
        new(EventType.Completed, Now, new NyaaSeriesBound(seriesId, $"Title {seriesId}", "SubsPlease", "(1080p)", origin, runId));

    private static EventData<NyaaBindFailed> Failed(string seriesId, Guid runId) =>
        new(EventType.Error, Now, new NyaaBindFailed(seriesId, "SubsPlease", $"Release {seriesId}", runId));

    #endregion
}
