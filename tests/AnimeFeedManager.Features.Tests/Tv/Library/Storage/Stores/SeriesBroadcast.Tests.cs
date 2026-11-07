using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.Storage.Stores;

public class SeriesBroadcastTests
{
    private static readonly DateTime Premier = new(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);

    #region ToExpectedEpisodes Tests

    [Fact]
    public void Should_Return_Known_When_Count_Is_Positive()
    {
        var result = SeriesBroadcast.ToExpectedEpisodes(12);

        Assert.Equal(new KnownEpisodes(12), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Return_Unknown_When_Count_Is_Missing_Or_Not_Positive(int? episodes)
    {
        var result = SeriesBroadcast.ToExpectedEpisodes(episodes);

        Assert.IsType<UnknownEpisodes>(result);
    }

    #endregion

    #region Release Slot Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-6)]
    [InlineData(-14)]
    [InlineData(7)]
    public void Should_Return_First_Release_When_Sub_Time_Is_Within_Allowance_Of_Premier(int daysFromPremier)
    {
        var subTime = Premier.AddDays(daysFromPremier).AddHours(15);

        var result = SeriesBroadcast.ToBroadcastHint(subTime, Premier, null);

        Assert.Equal(new FirstRelease(subTime), result.When);
    }

    [Theory]
    [InlineData(-15)]
    [InlineData(-90)]
    public void Should_Return_Unknown_When_Sub_Time_Is_A_Preseason_Placeholder(int daysFromPremier)
    {
        var result = SeriesBroadcast.ToBroadcastHint(Premier.AddDays(daysFromPremier), Premier, null);

        Assert.IsType<UnknownRelease>(result.When);
    }

    [Fact]
    public void Should_Return_Unknown_When_Sub_Time_Is_Missing()
    {
        var result = SeriesBroadcast.ToBroadcastHint(null, Premier, null);

        Assert.IsType<UnknownRelease>(result.When);
    }

    [Fact]
    public void Should_Return_Unknown_When_Premier_Is_Missing()
    {
        var result = SeriesBroadcast.ToBroadcastHint(Premier, null, null);

        Assert.IsType<UnknownRelease>(result.When);
    }

    #endregion

    #region Stream Link Tests

    [Fact]
    public void Should_Build_Https_Links_From_Stored_Streams()
    {
        var stored = Stored(
            new StoredStream("crunchyroll", "Crunchyroll", "www.crunchyroll.com/series/G1/show"),
            new StoredStream("netflix", "Netflix", "www.netflix.com/title/1"));

        var result = SeriesBroadcast.ToBroadcastHint(null, null, stored);

        Assert.Equal(
            [
                new StreamLink("crunchyroll", "Crunchyroll", new Uri("https://www.crunchyroll.com/series/G1/show")),
                new StreamLink("netflix", "Netflix", new Uri("https://www.netflix.com/title/1"))
            ],
            result.Where);
    }

    [Fact]
    public void Should_Keep_Url_When_It_Already_Has_A_Scheme()
    {
        var stored = Stored(new StoredStream("hidive", "HiDive", "https://www.hidive.com/season/1"));

        var result = SeriesBroadcast.ToBroadcastHint(null, null, stored);

        Assert.Equal(new Uri("https://www.hidive.com/season/1"), Assert.Single(result.Where).Url);
    }

    [Fact]
    public void Should_Fall_Back_To_Platform_When_Name_Is_Missing()
    {
        var stored = Stored(new StoredStream("youtube", null, "www.youtube.com/playlist?list=PL1"));

        var result = SeriesBroadcast.ToBroadcastHint(null, null, stored);

        Assert.Equal("youtube", Assert.Single(result.Where).Name);
    }

    [Fact]
    public void Should_Drop_Streams_Without_Platform_Or_Url()
    {
        var stored = Stored(
            new StoredStream(null, "Nameless", "www.example.com/a"),
            new StoredStream("amazon", "Amazon", null),
            new StoredStream("apple", "Apple TV", "apple.co/abc"));

        var result = SeriesBroadcast.ToBroadcastHint(null, null, stored);

        Assert.Equal("apple", Assert.Single(result.Where).Platform);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    public void Should_Return_No_Links_When_Stored_Streams_Are_Missing_Or_Malformed(string? stored)
    {
        var result = SeriesBroadcast.ToBroadcastHint(null, null, stored);

        Assert.Empty(result.Where);
    }

    #endregion

    #region Test Helpers

    private static string Stored(params StoredStream[] streams) => StoredStreams.ToStoredString(streams)!;

    #endregion
}
