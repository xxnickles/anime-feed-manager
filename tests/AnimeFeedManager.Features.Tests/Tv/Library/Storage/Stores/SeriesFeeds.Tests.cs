using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.Storage.Stores;

public class SeriesFeedsTests
{
    [Fact]
    public void Should_Project_Local_Feed_When_Source_Is_Local()
    {
        var series = MakeSeries("Psyren", "/tv/releases/psyren", SeriesFeeds.LocalSource);

        Assert.Equal(new LocalFeed("Psyren", "/tv/releases/psyren"), series.Feed);
    }

    [Fact]
    public void Should_Project_Local_Feed_When_Source_Is_Local_And_Title_Is_Missing()
    {
        var series = MakeSeries(null, null, SeriesFeeds.LocalSource);

        Assert.Equal(new LocalFeed(string.Empty, string.Empty), series.Feed);
    }

    [Fact]
    public void Should_Project_SubsPlease_Feed_When_Source_Is_SubsPlease()
    {
        var series = MakeSeries("Sword Warriors", "https://example.com/sword", SeriesFeeds.SubsPleaseSource);

        Assert.Equal(new SubsPleaseFeed("Sword Warriors", "https://example.com/sword"), series.Feed);
    }

    [Fact]
    public void Should_Project_SubsPlease_Feed_When_Feed_Title_Has_No_Source()
    {
        var series = MakeSeries("Sword Warriors", "https://example.com/sword", null);

        Assert.Equal(new SubsPleaseFeed("Sword Warriors", "https://example.com/sword"), series.Feed);
    }

    [Fact]
    public void Should_Project_SubsPlease_Feed_With_Empty_Link_When_Link_Is_Missing()
    {
        var series = MakeSeries("Sword Warriors", null, null);

        Assert.Equal(new SubsPleaseFeed("Sword Warriors", string.Empty), series.Feed);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, SeriesFeeds.SubsPleaseSource)]
    public void Should_Project_No_Feed_When_Feed_Title_Is_Blank_And_Source_Is_Not_Local(string? feedTitle,
        string? source)
    {
        var series = MakeSeries(feedTitle, "https://example.com/stale", source);

        Assert.IsType<NoFeed>(series.Feed);
    }

    private static AnimeInfoStorage MakeSeries(string? feedTitle, string? feedLink, string? source) =>
        new()
        {
            RowKey = "id",
            PartitionKey = "2026-fall",
            Title = "Title",
            FeedTitle = feedTitle,
            FeedLink = feedLink,
            FeedSource = source
        };
}
