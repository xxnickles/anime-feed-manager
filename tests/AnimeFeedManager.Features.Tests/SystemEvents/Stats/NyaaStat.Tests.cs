using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class NyaaStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_Return_Zeroes_When_There_Are_No_Events()
    {
        var result = NyaaStat.Summarize([], []);

        Assert.Equal(0, result.Bound);
        Assert.Equal(0, result.Backfilled);
        Assert.Empty(result.PossiblyTruncated);
    }

    [Fact]
    public void Should_Count_Every_Bind_And_Backfill_When_Series_Repeat()
    {
        var bound = ImmutableArray.Create(Bound("a", Now.AddDays(-1)), Bound("a", Now));
        var backfilled = ImmutableArray.Create(Backfill("a", Now.AddDays(-1)), Backfill("a", Now), Backfill("b", Now));

        var result = NyaaStat.Summarize(bound, backfilled);

        Assert.Equal(2, result.Bound);
        Assert.Equal(3, result.Backfilled);
    }

    [Fact]
    public void Should_List_Series_When_Its_Latest_Backfill_Is_Truncated()
    {
        var backfilled = ImmutableArray.Create(
            Backfill("a", Now.AddDays(-1)),
            Backfill("a", Now, truncated: true));

        var result = NyaaStat.Summarize([], backfilled);

        Assert.Equal([new TruncatedBackfill("a", "Title a", Now)], result.PossiblyTruncated);
    }

    [Fact]
    public void Should_Not_List_Series_When_A_Later_Backfill_Clears_The_Truncation()
    {
        var backfilled = ImmutableArray.Create(
            Backfill("a", Now.AddDays(-1), truncated: true),
            Backfill("a", Now));

        var result = NyaaStat.Summarize([], backfilled);

        Assert.Empty(result.PossiblyTruncated);
    }

    [Fact]
    public void Should_Order_Truncated_Series_Newest_First_When_Several_Are_Truncated()
    {
        var backfilled = ImmutableArray.Create(
            Backfill("old", Now.AddDays(-3), truncated: true),
            Backfill("new", Now, truncated: true));

        var result = NyaaStat.Summarize([], backfilled);

        Assert.Equal(["new", "old"], result.PossiblyTruncated.Select(t => t.SeriesId));
    }

    #region Test Helpers

    private static EventData<NyaaSeriesBound> Bound(string seriesId, DateTimeOffset at) =>
        new(EventType.Completed, at, new NyaaSeriesBound(seriesId, $"Title {seriesId}", "group", "1080p"));

    private static EventData<NyaaSeriesBackfilled> Backfill(string seriesId, DateTimeOffset at, bool truncated = false) =>
        new(EventType.Completed, at,
            new NyaaSeriesBackfilled(seriesId, $"Title {seriesId}", 12, truncated, Completed: false));

    #endregion
}
