using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Scrapping.AnimeSchedule;

public partial class AnimeScheduleTvScrapperTests
{
    #region Episodes

    [Fact]
    public async Task Episode_Count_Is_Stored()
    {
        var client = ClientReturning(CreateAnime() with { Episodes = 12 });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(12, Single(data).Series.Episodes));
    }

    [Fact]
    public async Task Missing_Episode_Count_Stays_Null()
    {
        var client = ClientReturning(CreateAnime() with { Episodes = null });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Null(Single(data).Series.Episodes));
    }

    #endregion

    #region Broadcast times

    [Fact]
    public async Task Sentinel_Broadcast_Times_Map_To_Null()
    {
        var client = ClientReturning(CreateAnime() with { JpnTime = DateTime.MinValue, SubTime = DateTime.MinValue });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            Assert.Null(Single(data).Series.JpnTime);
            Assert.Null(Single(data).Series.SubTime);
        });
    }

    [Fact]
    public async Task Real_Broadcast_Times_Are_Stored_As_Utc()
    {
        var jpnTime = new DateTime(2026, 10, 3, 14, 0, 0, DateTimeKind.Utc);
        var subTime = new DateTime(2026, 10, 3, 15, 30, 0, DateTimeKind.Utc);
        var client = ClientReturning(CreateAnime() with { JpnTime = jpnTime, SubTime = subTime });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            Assert.Equal(jpnTime, Single(data).Series.JpnTime);
            Assert.Equal(subTime, Single(data).Series.SubTime);
            Assert.Equal(DateTimeKind.Utc, Single(data).Series.SubTime!.Value.Kind);
        });
    }

    #endregion

    #region Streams

    [Fact]
    public async Task Streams_Are_Stored_With_Platform_Name_And_Url()
    {
        var client = ClientReturning(CreateAnime() with
        {
            Websites = new AnimeScheduleWebsites([
                new AnimeScheduleStream("crunchyroll", Name: "Crunchyroll", Url: "www.crunchyroll.com/series/G1/show"),
                new AnimeScheduleStream("netflix", Name: "Netflix", Url: "www.netflix.com/title/1")
            ])
        });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(
            [
                new StoredStream("crunchyroll", "Crunchyroll", "www.crunchyroll.com/series/G1/show"),
                new StoredStream("netflix", "Netflix", "www.netflix.com/title/1")
            ],
            StoredStreams.Parse(Single(data).Series.Streams)));
    }

    [Fact]
    public async Task Missing_Websites_Leaves_Streams_Null()
    {
        var client = ClientReturning(CreateAnime() with { Websites = null });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Null(Single(data).Series.Streams));
    }

    [Fact]
    public async Task Empty_Stream_List_Leaves_Streams_Null()
    {
        var client = ClientReturning(CreateAnime() with { Websites = new AnimeScheduleWebsites([]) });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Null(Single(data).Series.Streams));
    }

    #endregion

    #region Abbreviation

    [Fact]
    public async Task Abbreviation_Is_Stored_In_The_Provider_Block()
    {
        var client = ClientReturning(CreateAnime() with
        {
            Names = new AnimeScheduleNames("Romaji", "English", null, null, "PC2")
        });

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var titles = StoredAlternativeTitles.Parse(Single(data).Series.AlternativeTitles);
            Assert.Equal("PC2", titles.Abbreviation);
            Assert.Null(titles.User);
        });
    }

    #endregion
}
