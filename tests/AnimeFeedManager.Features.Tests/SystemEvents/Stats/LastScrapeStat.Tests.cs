using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class LastScrapeStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly SeriesSeason Fall = new(Season.Fall(), Year.FromNumber(2026));

    [Fact]
    public void Should_Return_NoScrape_When_There_Are_No_Events()
    {
        var result = LastScrapeStat.Summarize([]);

        Assert.IsType<NoScrape>(result);
    }

    [Fact]
    public void Should_Return_NoScrape_When_Only_Titles_Scrapes_Ran()
    {
        var result = LastScrapeStat.Summarize([Scrape(Now, UpdateType.Titles)]);

        Assert.IsType<NoScrape>(result);
    }

    [Fact]
    public void Should_Report_The_Latest_Library_Scrape_When_Several_Ran()
    {
        var events = ImmutableArray.Create(
            Scrape(Now.AddDays(-2), newSeries: 1, updatedSeries: 10),
            Scrape(Now, newSeries: 3, updatedSeries: 40),
            Scrape(Now.AddDays(-1), newSeries: 2, updatedSeries: 20));

        var result = Assert.IsType<LatestScrape>(LastScrapeStat.Summarize(events));

        Assert.Equal(Now, result.At);
        Assert.Equal(Fall, result.Season);
        Assert.Equal(3, result.NewSeries);
        Assert.Equal(40, result.UpdatedSeries);
        Assert.Equal(3, result.Runs);
    }

    [Fact]
    public void Should_Ignore_Titles_Scrapes_When_Library_Scrapes_Also_Ran()
    {
        var events = ImmutableArray.Create(
            Scrape(Now.AddDays(-1), newSeries: 2),
            Scrape(Now, UpdateType.Titles));

        var result = Assert.IsType<LatestScrape>(LastScrapeStat.Summarize(events));

        Assert.Equal(Now.AddDays(-1), result.At);
        Assert.Equal(1, result.Runs);
    }

    #region Test Helpers

    private static EventData<ScrapTvLibraryResult> Scrape(
        DateTimeOffset at,
        UpdateType updateType = UpdateType.FullLibrary,
        int newSeries = 0,
        int updatedSeries = 0) =>
        new(EventType.Completed, at, new ScrapTvLibraryResult(Fall, updatedSeries, newSeries, updateType));

    #endregion
}
