using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.SeriesReleases;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Queries;

public class ReleaseListingQueryTests
{
    private const string SeriesId = "2026_fall_psyren";
    private static readonly SeriesSeason Fall = new(Season.Fall(), Year.FromNumber(2026));
    private static readonly DateTimeOffset Published = new(2026, 10, 3, 15, 32, 0, TimeSpan.Zero);

    [Fact]
    public async Task Should_Fail_Not_Found_When_Series_Is_Not_In_The_Library()
    {
        var result = await GetListing(
            Task.FromResult(Result<TvSeries>.Failure(NotFoundError.Create("missing"))),
            Bound());

        result.AssertOnError(error => Assert.IsType<NotFoundError>(error));
    }

    [Fact]
    public async Task Should_Have_No_Release_Feed_When_Series_Has_No_Binding()
    {
        var result = await GetListing(Found(MakeSeries()), Task.FromResult(Result<NyaaBindingStorage>.Failure(NotFoundError.Create("unbound"))));

        result.AssertOnSuccess(listing => Assert.IsType<NoReleaseFeed>(listing.Feed));
    }

    [Fact]
    public async Task Should_Fail_When_Binding_Read_Fails_For_Another_Reason()
    {
        var result = await GetListing(Found(MakeSeries()), Task.FromResult(Result<NyaaBindingStorage>.Failure(Error.Create("storage down"))));

        result.AssertError();
    }

    [Fact]
    public async Task Should_Shape_The_Nyaa_Feed_When_Series_Is_Bound()
    {
        var result = await GetListing(Found(MakeSeries()), Bound(), [Row(1, version: 1), Row(1, version: 2), Row(2, version: 1)]);

        result.AssertOnSuccess(listing =>
        {
            var feed = Assert.IsType<NyaaReleaseFeed>(listing.Feed);
            Assert.Equal("SubsPlease", feed.Group);
            Assert.Equal("(1080p)", feed.Variant);
            Assert.True(feed.Trusted);
            Assert.Equal("https://nyaa.si/?c=1_2&f=2&q=%22%5BSubsPlease%5D%20Psyren%22", feed.Browse.AbsoluteUri);
            Assert.Equal(2, feed.Indexed);
            Assert.Equal(["02", "01"], feed.Episodes.Select(e => e.Label));
            Assert.Equal(new LastRelease(Published), feed.Cadence);
        });
    }

    [Fact]
    public async Task Should_Carry_Library_Details_When_Series_Is_Found()
    {
        var series = MakeSeries() with {Status = SeriesStatus.Completed(), Episodes = new KnownEpisodes(12)};

        var result = await GetListing(Found(series), Bound());

        result.AssertOnSuccess(listing =>
        {
            Assert.Equal(SeriesId, listing.SeriesId);
            Assert.Equal("Psyren", listing.Title);
            Assert.Equal(SeriesStatus.Completed(), listing.Status);
            Assert.Equal(new KnownEpisodes(12), listing.Episodes);
        });
    }

    [Fact]
    public async Task Should_List_Other_Titles_Without_The_Main_Title_Or_Duplicates()
    {
        var series = MakeSeries() with
        {
            AlternativeTitles = new AlternativeTitlesData(
                Romaji: "psyren",
                English: "Psyren: Another World",
                Native: "サイレン",
                Synonyms: ["PSYREN: Another World", "Psyren TV"],
                Abbreviation: "  ")
        };

        var result = await GetListing(Found(series), Bound());

        result.AssertOnSuccess(listing =>
            Assert.Equal(["Psyren: Another World", "Psyren TV", "サイレン"], listing.OtherTitles));
    }

    [Fact]
    public async Task Should_Fail_When_Series_Id_Has_No_Season()
    {
        var result = await GetListing(Found(MakeSeries()), Bound(), seriesId: "psyren");

        result.AssertError();
    }

    #region Test Helpers

    private static Task<Result<ReleaseListing>> GetListing(
        Task<Result<TvSeries>> series,
        Task<Result<NyaaBindingStorage>> binding,
        ImmutableArray<NyaaCatalogStorage> catalog = default,
        string seriesId = SeriesId) =>
        ReleaseListingQuery.GetReleaseListing(
            seriesId,
            (_, _, _, _) => series,
            new Uri("https://blob.example.com"),
            (_, _, _) => binding,
            (_, _) => Task.FromResult(Result<ImmutableArray<NyaaCatalogStorage>>.Success(catalog.IsDefault ? [] : catalog)),
            CancellationToken.None);

    private static Task<Result<TvSeries>> Found(TvSeries series) => Task.FromResult(Result<TvSeries>.Success(series));

    private static Task<Result<NyaaBindingStorage>> Bound() =>
        Task.FromResult(Result<NyaaBindingStorage>.Success(new NyaaBindingStorage
        {
            PartitionKey = "2026-fall",
            RowKey = SeriesId,
            Group = "SubsPlease",
            ReleaseTitle = "Psyren",
            Variant = "(1080p)",
            Trusted = true,
            Tracking = true
        }));

    private static TvSeries MakeSeries() => new(
        SeriesId,
        Fall,
        "Psyren",
        "synopsis",
        "Psyren",
        AlternativeTitlesData.Empty,
        SeriesStatus.Ongoing(),
        null,
        null,
        new UnknownEpisodes(),
        new BroadcastHint(new UnknownRelease(), []));

    private static NyaaCatalogStorage Row(int episode, int version) => new()
    {
        PartitionKey = SeriesId,
        RowKey = $"{episode:0000}-{version}",
        Title = $"[SubsPlease] Psyren - {episode:00} (1080p)",
        Torrent = $"https://nyaa.si/download/{episode * 10 + version}.torrent",
        Published = Published,
        Version = version,
        EpisodeNumber = episode,
        EpisodeLabel = $"{episode:00}"
    };

    #endregion
}
