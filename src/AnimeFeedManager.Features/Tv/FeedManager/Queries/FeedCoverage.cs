namespace AnimeFeedManager.Features.Tv.FeedManager.Queries;

/// <summary>A season's series counted by feed state; every bound series, stale included, counts as fed.</summary>
public sealed record FeedCoverage(int Tracking, int Completed, int Stale, int Unfed)
{
    public int Fed => Tracking + Completed + Stale;
    public int Total => Fed + Unfed;

    public static FeedCoverage From(IEnumerable<SeasonFeed> feeds) =>
        feeds.Aggregate(new FeedCoverage(0, 0, 0, 0), (coverage, feed) => feed switch
        {
            UnfedSeries => coverage with { Unfed = coverage.Unfed + 1 },
            BoundSeries { State: BindingState.Stale } => coverage with { Stale = coverage.Stale + 1 },
            BoundSeries { State: BindingState.Completed } => coverage with { Completed = coverage.Completed + 1 },
            BoundSeries => coverage with { Tracking = coverage.Tracking + 1 }
        });

    /// <summary>Slice colors match the feed list's row badges.</summary>
    public DonutChart ToDonutChart() => new(
    [
        new ChartSlice("Tracking", Tracking, ChartColor.Success),
        new ChartSlice("Completed", Completed, ChartColor.Neutral),
        new ChartSlice("Stale", Stale, ChartColor.Warning),
        new ChartSlice("No feed", Unfed, ChartColor.BaseContent)
    ]);
}
