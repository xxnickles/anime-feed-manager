using System.Globalization;
using AnimeFeedManager.Features.Tv.FeedManager.SeriesReleases;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.SeriesReleases;

public class NyaaReleasesTests
{
    // A Saturday.
    private static readonly DateTimeOffset Premiere = new(2026, 10, 3, 15, 32, 0, TimeSpan.Zero);

    #region ByEpisode Tests

    [Fact]
    public void Should_Return_Empty_When_Catalog_Is_Empty()
    {
        Assert.Empty(NyaaReleases.ByEpisode([]));
    }

    [Fact]
    public void Should_Order_Episodes_Newest_First_When_Catalog_Is_Unordered()
    {
        var episodes = NyaaReleases.ByEpisode([Row(1, Week(0)), Row(3, Week(2)), Row(2, Week(1))]);

        Assert.Equal(["03", "02", "01"], episodes.Select(e => e.Label));
    }

    [Fact]
    public void Should_Place_Special_Between_Its_Neighbours_When_Ordering()
    {
        var episodes = NyaaReleases.ByEpisode([Row(5, Week(4)), Special(5.5, Week(4).AddDays(3)), Row(6, Week(5))]);

        Assert.Equal(["06", "05.5", "05"], episodes.Select(e => e.Label));
        Assert.Equal([false, true, false], episodes.Select(e => e.Special));
    }

    [Fact]
    public void Should_Fold_Older_Version_Under_Latest_When_Episode_Is_Revised()
    {
        var episode = Assert.Single(NyaaReleases.ByEpisode([Row(5, Week(4), version: 1), Row(5, Week(4).AddDays(1), version: 2)]));

        Assert.Equal(2, episode.Latest.Version);
        Assert.Equal(1, Assert.Single(episode.Earlier).Version);
    }

    [Fact]
    public void Should_Order_Earlier_Versions_Highest_First_When_Several_Revisions_Exist()
    {
        var episode = Assert.Single(NyaaReleases.ByEpisode(
            [Row(5, Week(4), version: 1), Row(5, Week(4).AddDays(2), version: 3), Row(5, Week(4).AddDays(1), version: 2)]));

        Assert.Equal(3, episode.Latest.Version);
        Assert.Equal([2, 1], episode.Earlier.Select(r => r.Version));
    }

    [Fact]
    public void Should_Pick_Newest_Upload_As_Latest_When_Versions_Tie()
    {
        var episode = Assert.Single(NyaaReleases.ByEpisode(
            [Row(5, Week(4), torrentId: 100), Row(5, Week(4).AddHours(3), torrentId: 200)]));

        Assert.Equal(Week(4).AddHours(3), episode.Latest.Published);
        Assert.Equal(Week(4), Assert.Single(episode.Earlier).Published);
    }

    [Fact]
    public void Should_Read_Revised_Row_As_Version_2_When_Version_Is_Not_Stored()
    {
        var revised = Row(5, Week(4).AddDays(1));
        revised.Version = null;
        revised.Revised = true;

        var episode = Assert.Single(NyaaReleases.ByEpisode([Row(5, Week(4)), revised]));

        Assert.Equal(2, episode.Latest.Version);
    }

    [Fact]
    public void Should_Carry_The_Nyaa_View_Page_When_Release_Is_Shaped()
    {
        var episode = Assert.Single(NyaaReleases.ByEpisode([Row(1, Week(0), torrentId: 2041837)]));

        Assert.Equal(new Uri("https://nyaa.si/view/2041837"), episode.Latest.View);
    }

    #endregion

    #region Cadence Tests

    [Fact]
    public void Should_Have_No_Releases_When_Catalog_Is_Empty()
    {
        Assert.IsType<NoReleases>(NyaaReleases.Cadence([]));
    }

    [Fact]
    public void Should_Be_Weekly_At_The_Newest_Slot_When_Three_Episodes_Are_A_Week_Apart()
    {
        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(1)), Row(3, Week(2))]);

        Assert.Equal(new WeeklyCadence(DayOfWeek.Saturday, new TimeOnly(15, 30)), cadence);
    }

    [Fact]
    public void Should_Round_Slot_Up_When_Time_Is_Past_The_Quarter()
    {
        var newest = Week(2).AddMinutes(14); // 15:46

        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(1)), Row(3, newest)]);

        Assert.Equal(new WeeklyCadence(DayOfWeek.Saturday, new TimeOnly(16, 0)), cadence);
    }

    [Fact]
    public void Should_Roll_Slot_To_Next_Day_When_Rounding_Crosses_Midnight()
    {
        var lateNight = new DateTimeOffset(2026, 10, 3, 23, 50, 0, TimeSpan.Zero);

        var cadence = NyaaReleases.Cadence([Row(1, lateNight), Row(2, lateNight.AddDays(7)), Row(3, lateNight.AddDays(14))]);

        Assert.Equal(new WeeklyCadence(DayOfWeek.Sunday, new TimeOnly(0, 0)), cadence);
    }

    [Fact]
    public void Should_Use_Utc_Slot_When_Published_Carries_An_Offset()
    {
        var tokyo = Week(0).ToOffset(TimeSpan.FromHours(9));

        var cadence = NyaaReleases.Cadence([Row(1, tokyo), Row(2, tokyo.AddDays(7)), Row(3, tokyo.AddDays(14))]);

        Assert.Equal(new WeeklyCadence(DayOfWeek.Saturday, new TimeOnly(15, 30)), cadence);
    }

    [Fact]
    public void Should_Show_Last_Release_When_Fewer_Than_Three_Regular_Episodes()
    {
        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(1))]);

        Assert.Equal(new LastRelease(Week(1)), cadence);
    }

    [Fact]
    public void Should_Show_Last_Release_When_Gaps_Are_Irregular()
    {
        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(2)), Row(3, Week(4))]);

        Assert.Equal(new LastRelease(Week(4)), cadence);
    }

    [Fact]
    public void Should_Be_Weekly_When_Median_Gap_Is_Exactly_Eight_Days()
    {
        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(0).AddDays(8)), Row(3, Week(0).AddDays(16))]);

        Assert.IsType<WeeklyCadence>(cadence);
    }

    [Fact]
    public void Should_Show_Last_Release_When_Median_Gap_Exceeds_Eight_Days()
    {
        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(0).AddDays(9)), Row(3, Week(0).AddDays(18))]);

        Assert.IsType<LastRelease>(cadence);
    }

    [Fact]
    public void Should_Ignore_Revisions_When_Measuring_Cadence()
    {
        var cadence = NyaaReleases.Cadence(
            [Row(1, Week(0)), Row(2, Week(1)), Row(3, Week(2)), Row(3, Week(2).AddDays(2), version: 2)]);

        Assert.Equal(new WeeklyCadence(DayOfWeek.Saturday, new TimeOnly(15, 30)), cadence);
    }

    [Fact]
    public void Should_Ignore_Specials_When_Measuring_Cadence()
    {
        var cadence = NyaaReleases.Cadence(
            [Row(1, Week(0)), Row(2, Week(1)), Special(2.5, Week(1).AddDays(3)), Row(3, Week(2))]);

        Assert.IsType<WeeklyCadence>(cadence);
    }

    [Fact]
    public void Should_Measure_Only_The_Last_Four_Episodes_When_Early_Gaps_Are_Irregular()
    {
        var start = Week(0).AddDays(-40);

        var cadence = NyaaReleases.Cadence(
            [Row(1, start), Row(2, start.AddDays(5)), Row(3, Week(0)), Row(4, Week(1)), Row(5, Week(2)), Row(6, Week(3))]);

        Assert.IsType<WeeklyCadence>(cadence);
    }

    [Fact]
    public void Should_Show_Newest_Release_Of_Any_Kind_When_Not_Weekly()
    {
        var newest = Week(1).AddDays(3);

        var cadence = NyaaReleases.Cadence([Row(1, Week(0)), Row(2, Week(1)), Special(2.5, newest)]);

        Assert.Equal(new LastRelease(newest), cadence);
    }

    [Fact]
    public void Should_Show_Last_Release_When_Catalog_Holds_Only_Specials()
    {
        var cadence = NyaaReleases.Cadence([Special(0, Week(0)), Special(0.5, Week(1))]);

        Assert.Equal(new LastRelease(Week(1)), cadence);
    }

    #endregion

    #region ViewPage Tests

    [Fact]
    public void Should_Derive_View_Page_When_Torrent_Is_A_Nyaa_Download()
    {
        Assert.Equal(new Uri("https://nyaa.si/view/123456"), NyaaReleases.ViewPage("https://nyaa.si/download/123456.torrent"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("https://nyaa.si/view/123456")]
    [InlineData("https://nyaa.si/download/abc.torrent")]
    [InlineData("https://nyaa.si/download/.torrent")]
    [InlineData("https://nyaa.si/download/123456.zip")]
    public void Should_Have_No_View_Page_When_Torrent_Is_Not_A_Nyaa_Download(string torrent)
    {
        Assert.Null(NyaaReleases.ViewPage(torrent));
    }

    #endregion

    #region Test Helpers

    private static DateTimeOffset Week(int weeks) => Premiere.AddDays(7 * weeks);

    private static NyaaCatalogStorage Row(double episode, DateTimeOffset published, int version = 1, int? torrentId = null) =>
        CatalogRow(episode, false, published, version, torrentId);

    private static NyaaCatalogStorage Special(double episode, DateTimeOffset published) =>
        CatalogRow(episode, true, published, 1, null);

    private static NyaaCatalogStorage CatalogRow(
        double episode,
        bool special,
        DateTimeOffset published,
        int version,
        int? torrentId)
    {
        var label = episode.ToString("00.####", CultureInfo.InvariantCulture);
        var id = torrentId ?? (int) (episode * 100) + version;
        return new NyaaCatalogStorage
        {
            PartitionKey = "series-id",
            RowKey = $"{label}-{id}",
            Title = $"[SubsPlease] Show - {label}{(version > 1 ? $"v{version}" : string.Empty)} (1080p)",
            Magnet = $"magnet:?xt=urn:btih:{id}",
            Torrent = $"https://nyaa.si/download/{id}.torrent",
            Published = published,
            Size = "1.4 GiB",
            Revised = version > 1,
            Version = version,
            EpisodeNumber = episode,
            EpisodeLabel = label,
            Special = special
        };
    }

    #endregion
}
