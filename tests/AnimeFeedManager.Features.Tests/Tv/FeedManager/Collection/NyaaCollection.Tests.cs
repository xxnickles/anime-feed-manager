using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Collection;

public class NyaaCollectionTests
{
    private static readonly DateTimeOffset Premiere = new(2026, 10, 2, 15, 0, 0, TimeSpan.Zero);
    private static readonly WebAppHost Host = new(new Uri("https://afm.example/"));

    #region Match Tests

    [Fact]
    public void Should_Return_No_Matches_When_No_Item_Matches_A_Binding()
    {
        var result = NyaaCollection.Match([Binding("2026_fall_show", "Show")], [Item("Other", "01")]);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Give_Each_Binding_Only_Its_Own_Releases()
    {
        var result = NyaaCollection.Match(
            [Binding("2026_fall_a", "Show A"), Binding("2026_fall_b", "Show B")],
            [Item("Show A", "01", "hash-a"), Item("Show B", "01", "hash-b"), Item("Show B", "02", "hash-b2")]);

        Assert.Equal(["hash-a"], result.Single(match => match.Binding.RowKey == "2026_fall_a").Rows.Select(row => row.InfoHash));
        Assert.Equal(["hash-b", "hash-b2"],
            result.Single(match => match.Binding.RowKey == "2026_fall_b").Rows.Select(row => row.InfoHash).Order());
    }

    [Fact]
    public void Should_Leave_Out_Bindings_Without_Matches()
    {
        var result = NyaaCollection.Match(
            [Binding("2026_fall_a", "Show A"), Binding("2026_fall_b", "Show B")],
            [Item("Show A", "01")]);

        Assert.Equal("2026_fall_a", Assert.Single(result).Binding.RowKey);
    }

    [Fact]
    public void Should_Skip_Releases_Published_Before_A_Bindings_Premiere()
    {
        var result = NyaaCollection.Match(
            [Binding("2026_fall_show", "Show", Premiere)],
            [Item("Show", "24", published: Premiere.AddDays(-30))]);

        Assert.Empty(result);
    }

    #endregion

    #region Feeds Tests

    [Fact]
    public void Should_Return_No_Feeds_When_Nothing_Was_Collected()
    {
        Assert.Empty(NyaaCollection.Feeds([], Host));
    }

    [Fact]
    public void Should_Link_Feed_To_The_Series_Releases_Page()
    {
        var feed = Assert.Single(NyaaCollection.Feeds([Collected("2026_fall_show", "Show", [Row(1)])], Host));

        Assert.Equal("Show", feed.Title);
        Assert.Equal("https://afm.example/tv/releases/2026_fall_show", feed.Url);
    }

    [Fact]
    public void Should_Order_Episodes_Newest_First()
    {
        var feed = Assert.Single(NyaaCollection.Feeds(
            [Collected("2026_fall_show", "Show", [Row(1), Row(3), Row(2)])], Host));

        Assert.Equal(["03", "02", "01"], feed.Episodes.Select(episode => episode.EpisodeNumber));
    }

    [Fact]
    public void Should_Keep_A_Revision_As_Its_Own_Episode()
    {
        var feed = Assert.Single(NyaaCollection.Feeds(
            [Collected("2026_fall_show", "Show", [Row(5), Row(5, version: 2)])], Host));

        Assert.Equal(["05v2", "05"], feed.Episodes.Select(episode => episode.EpisodeNumber));
    }

    [Fact]
    public void Should_Keep_The_Newest_Upload_When_An_Episode_Repeats()
    {
        var feed = Assert.Single(NyaaCollection.Feeds(
            [
                Collected("2026_fall_show", "Show",
                [
                    Row(5, infoHash: "older", published: Premiere),
                    Row(5, infoHash: "newer", published: Premiere.AddHours(1))
                ])
            ], Host));

        Assert.Equal("magnet:newer", Assert.Single(feed.Episodes).MagnetLink);
    }

    [Fact]
    public void Should_Cap_Feed_At_The_Ten_Latest_Episodes()
    {
        var feed = Assert.Single(NyaaCollection.Feeds(
            [Collected("2026_fall_show", "Show", [.. Enumerable.Range(1, 12).Select(episode => Row(episode))])], Host));

        Assert.Equal(NyaaCollection.FeedEpisodes, feed.Episodes.Length);
        Assert.Equal("12", feed.Episodes[0].EpisodeNumber);
        Assert.Equal("03", feed.Episodes[^1].EpisodeNumber);
    }

    [Fact]
    public void Should_Flag_Only_Releases_Matched_This_Run_As_New()
    {
        var feed = Assert.Single(NyaaCollection.Feeds(
            [Collected("2026_fall_show", "Show", [Row(1, infoHash: "old"), Row(2, infoHash: "new")], matched: ["new"])],
            Host));

        Assert.Equal([("02", true), ("01", false)],
            feed.Episodes.Select(episode => (episode.EpisodeNumber, episode.IsNew)));
    }

    [Fact]
    public void Should_Keep_The_Newest_Season_When_Series_Share_A_Feed_Title()
    {
        var feeds = NyaaCollection.Feeds(
            [
                Collected("2026_summer_show", "Show", [Row(13, infoHash: "summer")]),
                Collected("2026_fall_show-s2", "Show", [Row(1, infoHash: "fall")])
            ], Host);

        Assert.Equal("https://afm.example/tv/releases/2026_fall_show-s2", Assert.Single(feeds).Url);
    }

    [Fact]
    public void Should_Skip_Series_Without_A_Feed_Title()
    {
        Assert.Empty(NyaaCollection.Feeds([Collected("2026_fall_show", "", [Row(1)])], Host));
    }

    [Fact]
    public void Should_Skip_Series_With_An_Empty_Catalog()
    {
        Assert.Empty(NyaaCollection.Feeds([Collected("2026_fall_show", "Show", [])], Host));
    }

    #endregion

    #region Test Helpers

    private static NyaaBindingStorage Binding(string seriesId, string releaseTitle, DateTimeOffset? premiere = null) =>
        new()
        {
            PartitionKey = "2026-fall",
            RowKey = seriesId,
            Group = "SubsPlease",
            ReleaseTitle = releaseTitle,
            Variant = "(1080p)",
            FeedTitle = releaseTitle,
            Trusted = true,
            Tracking = true,
            Premiere = premiere
        };

    private static NyaaItem Item(string title, string episode, string infoHash = "hash", DateTimeOffset? published = null) =>
        new($"[SubsPlease] {title} - {episode} (1080p) [ABCD1234].mkv", $"magnet:{infoHash}",
            "https://nyaa.si/download/1.torrent", infoHash, published ?? Premiere.AddDays(7), "1.4 GiB",
            Trusted: true, Remake: false, Category: "1_2");

    private static CollectedSeries Collected(string seriesId, string feedTitle,
        ImmutableArray<NyaaCatalogStorage> catalog, ImmutableArray<string> matched = default) =>
        new(Binding(seriesId, feedTitle), catalog,
            [.. matched.IsDefault ? [] : matched]);

    private static NyaaCatalogStorage Row(int episode, int? version = null, string? infoHash = null,
        DateTimeOffset? published = null)
    {
        var hash = infoHash ?? $"hash-{episode}-{version}";
        return new()
        {
            EpisodeNumber = episode,
            Version = version,
            InfoHash = hash,
            Magnet = $"magnet:{hash}",
            Torrent = $"https://nyaa.si/download/{hash}.torrent",
            Published = published ?? Premiere.AddDays(episode * 7)
        };
    }

    #endregion
}
