using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Queries;

public class RecentReleasesQueryTests
{
    private static readonly DateTimeOffset Since = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    #region Summarize Tests

    [Fact]
    public void Should_Return_Empty_When_Nothing_Is_Tracked()
    {
        Assert.Empty(RecentReleasesQuery.Summarize([], Since));
    }

    [Fact]
    public void Should_Skip_Series_When_No_Release_Is_Within_The_Window()
    {
        var result = RecentReleasesQuery.Summarize(
            [Catalog("2026_fall_show", "Show", Row(1, Since.AddHours(-1)))], Since);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Keep_Only_Releases_Within_The_Window_When_Catalog_Has_Older_Ones()
    {
        var result = RecentReleasesQuery.Summarize(
            [Catalog("2026_fall_show", "Show", Row(1, Since.AddDays(-7)), Row(2, Since), Row(3, Since.AddHours(5)))],
            Since);

        var series = Assert.Single(result);
        Assert.Equal(["03", "02"], series.Episodes.Select(episode => episode.Episode));
    }

    [Fact]
    public void Should_Skip_Series_When_Feed_Title_Is_Blank()
    {
        var result = RecentReleasesQuery.Summarize(
            [Catalog("2026_fall_untitled", " ", Row(1, Since.AddHours(1)))], Since);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Keep_Newest_Upload_When_An_Episode_Label_Repeats()
    {
        var result = RecentReleasesQuery.Summarize(
            [Catalog("2026_fall_show", "Show", Row(4, Since.AddHours(1), "hash-a"), Row(4, Since.AddHours(3), "hash-b"))],
            Since);

        var episode = Assert.Single(Assert.Single(result).Episodes);
        Assert.Equal(Since.AddHours(3), episode.Published);
    }

    [Fact]
    public void Should_Order_Episodes_By_Number_Then_Version_When_Several_Are_Recent()
    {
        var result = RecentReleasesQuery.Summarize(
            [Catalog("2026_fall_show", "Show", Row(5, Since.AddHours(1)), Row(6, Since.AddHours(2)),
                Row(5, Since.AddHours(3), "hash-v2", version: 2))],
            Since);

        Assert.Equal(["06", "05v2", "05"], Assert.Single(result).Episodes.Select(episode => episode.Episode));
    }

    [Fact]
    public void Should_Order_Series_By_Newest_Release_When_Several_Have_Recent_Releases()
    {
        var result = RecentReleasesQuery.Summarize(
            [
                Catalog("2026_fall_early", "Early", Row(1, Since.AddHours(1))),
                Catalog("2026_fall_late", "Late", Row(1, Since.AddHours(2)), Row(2, Since.AddHours(8)))
            ],
            Since);

        Assert.Equal(["2026_fall_late", "2026_fall_early"], result.Select(series => series.SeriesId));
        Assert.Equal(Since.AddHours(8), result[0].Latest);
        Assert.Equal("Late", result[0].Title);
    }

    #endregion

    #region Test Helpers

    private static SeriesCatalog Catalog(string seriesId, string feedTitle, params NyaaCatalogStorage[] rows) =>
        new(new NyaaBindingStorage {RowKey = seriesId, FeedTitle = feedTitle, Tracking = true}, [.. rows]);

    private static NyaaCatalogStorage Row(int episode, DateTimeOffset published, string? infoHash = null,
        int? version = null) =>
        new()
        {
            InfoHash = infoHash ?? $"hash-{episode}",
            EpisodeNumber = episode,
            EpisodeLabel = episode.ToString("00"),
            Published = published,
            Version = version
        };

    #endregion
}
