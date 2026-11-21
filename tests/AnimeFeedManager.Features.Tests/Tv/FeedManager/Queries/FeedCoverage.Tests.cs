using AnimeFeedManager.Shared.Charts;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Queries;

public class FeedCoverageTests
{
    #region From Tests

    [Fact]
    public void Should_Count_Nothing_When_Season_Has_No_Feeds()
    {
        var coverage = FeedCoverage.From([]);

        Assert.Equal(new FeedCoverage(0, 0, 0, 0), coverage);
        Assert.Equal(0, coverage.Total);
    }

    [Fact]
    public void Should_Count_Each_State_When_Feeds_Are_Mixed()
    {
        SeasonFeed[] feeds =
        [
            Bound(BindingState.Tracking), Bound(BindingState.Tracking),
            Bound(BindingState.Completed),
            Bound(BindingState.Stale),
            Unfed(), Unfed(), Unfed()
        ];

        var coverage = FeedCoverage.From(feeds);

        Assert.Equal(new FeedCoverage(Tracking: 2, Completed: 1, Stale: 1, Unfed: 3), coverage);
        Assert.Equal(4, coverage.Fed);
        Assert.Equal(7, coverage.Total);
    }

    [Fact]
    public void Should_Count_Stale_As_Fed_When_Series_Is_Bound_But_Stale()
    {
        var coverage = FeedCoverage.From([Bound(BindingState.Stale)]);

        Assert.Equal(1, coverage.Fed);
    }

    #endregion

    #region ToDonutChart Tests

    [Fact]
    public void Should_Chart_One_Slice_Per_State_With_Badge_Colors_When_Charted()
    {
        var chart = new FeedCoverage(Tracking: 5, Completed: 2, Stale: 1, Unfed: 3).ToDonutChart();

        Assert.Equal(
            [
                new ChartSlice("Tracking", 5, ChartColor.Success),
                new ChartSlice("Completed", 2, ChartColor.Neutral),
                new ChartSlice("Stale", 1, ChartColor.Warning),
                new ChartSlice("No feed", 3, ChartColor.BaseContent)
            ],
            chart.Slices);
    }

    #endregion

    #region Test Helpers

    private static BoundSeries Bound(BindingState state) =>
        new("2026_fall_psyren", "Psyren", "SubsPlease", 3, new KnownEpisodes(12), state);

    private static UnfedSeries Unfed() => new("2026_fall_tougen", "Tougen Anki", new UnknownEpisodes());

    #endregion
}
