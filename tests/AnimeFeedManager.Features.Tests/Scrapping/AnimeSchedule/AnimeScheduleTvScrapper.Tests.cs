using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;

namespace AnimeFeedManager.Features.Tests.Scrapping.AnimeSchedule;

public class AnimeScheduleTvScrapperTests
{
    private const string NoSynopsis = "No synopsis available.";

    #region Season

    [Fact]
    public async Task Fetches_The_Given_Season()
    {
        var client = ClientReturning(CreateAnime(title: "X"));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        _ = client.Received(1).GetSeason(2026, "spring", Arg.Any<CancellationToken>());
        result.AssertOnSuccess(data => Assert.Single(data.SeriesData));
    }

    [Fact]
    public async Task Series_Are_Stored_Under_The_Given_Season()
    {
        var client = ClientReturning(CreateAnime(title: "X"));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            Assert.Equal(Spring2026, data.Season);
            Assert.Equal("2026-spring", Single(data).Series.PartitionKey);
        });
    }

    [Fact]
    public async Task Client_Failure_Propagates()
    {
        var client = Substitute.For<IAnimeScheduleClient>();
        client.GetSeason(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ImmutableArray<AnimeScheduleAnime>>.Failure(HandledError.Create())));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertError();
    }

    #endregion

    #region Media type filtering

    [Fact]
    public async Task Tv_And_TvShort_Are_Kept_Other_Media_Types_Dropped()
    {
        var client = ClientReturning(
            CreateAnime(title: "Series", mediaTypeRoute: "tv"),
            CreateAnime(title: "Short", mediaTypeRoute: "tv-short"),
            CreateAnime(title: "Film", mediaTypeRoute: "movie"),
            CreateAnime(title: "Web", mediaTypeRoute: "ona"),
            CreateAnime(title: "Extra", mediaTypeRoute: "special"));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var titles = data.SeriesData.Select(entry => entry.Series.Title).ToImmutableArray();
            Assert.Equal(2, titles.Length);
            Assert.Contains("Series", titles);
            Assert.Contains("Short", titles);
        });
    }

    [Fact]
    public async Task Entry_Without_Media_Types_Is_Dropped()
    {
        var client = ClientReturning(
            CreateAnime(title: "Kept", mediaTypeRoute: "tv"),
            CreateAnime(title: "Untyped", mediaTypes: []));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Single(data.SeriesData, entry => entry.Series.Title == "Kept"));
    }

    #endregion

    #region Synopsis

    [Fact]
    public async Task Description_Markup_Is_Stripped()
    {
        var client = ClientReturning(CreateAnime(
            description: """The third season of <span class="italics">Some Show</span>."""));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
            Assert.Equal("The third season of Some Show.", Single(data).Series.Synopsis));
    }

    [Fact]
    public async Task Line_Break_Tags_Become_Line_Breaks()
    {
        var client = ClientReturning(CreateAnime(description: "First.<br><br>Second."));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal("First.\n\nSecond.", Single(data).Series.Synopsis));
    }

    [Fact]
    public async Task Html_Entities_Are_Decoded()
    {
        var client = ClientReturning(CreateAnime(description: "Bread &amp; Butter &#39;s tale"));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal("Bread & Butter 's tale", Single(data).Series.Synopsis));
    }

    [Fact]
    public async Task Missing_Description_Falls_Back_To_Placeholder()
    {
        var client = ClientReturning(CreateAnime(description: null));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(NoSynopsis, Single(data).Series.Synopsis));
    }

    [Fact]
    public async Task Description_That_Is_Only_Markup_Falls_Back_To_Placeholder()
    {
        var client = ClientReturning(CreateAnime(description: "<span></span>   "));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(NoSynopsis, Single(data).Series.Synopsis));
    }

    #endregion

    #region Premier date

    [Fact]
    public async Task Sentinel_Premier_Maps_To_Null_Date()
    {
        var client = ClientReturning(CreateAnime(premier: DateTime.MinValue));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Null(Single(data).Series.Date));
    }

    [Fact]
    public async Task Missing_Premier_Maps_To_Null_Date()
    {
        var client = ClientReturning(CreateAnime(premier: null));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Null(Single(data).Series.Date));
    }

    [Fact]
    public async Task Real_Premier_Is_Stored_As_Utc()
    {
        var premier = new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc);
        var client = ClientReturning(CreateAnime(premier: premier));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var date = Single(data).Series.Date;
            Assert.Equal(premier, date);
            Assert.Equal(DateTimeKind.Utc, date!.Value.Kind);
        });
    }

    #endregion

    #region Image

    [Fact]
    public async Task Image_Url_Is_Built_From_Image_Version_Route()
    {
        var client = ClientReturning(CreateAnime(imageVersionRoute: "anime/jpg/default/show-abc.jpg"));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var image = Assert.IsType<ScrappedImageUrl>(Single(data).Image);
            Assert.Equal(
                "https://img.animeschedule.net/production/assets/public/img/anime/jpg/default/show-abc.jpg",
                image.Url.ToString());
        });
    }

    [Fact]
    public async Task Missing_Image_Version_Route_Yields_No_Image()
    {
        var client = ClientReturning(CreateAnime(imageVersionRoute: null));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.IsType<NoImage>(Single(data).Image));
    }

    #endregion

    #region Airing status

    [Theory]
    [InlineData("Upcoming", AiringStatus.Upcoming)]
    [InlineData("Ongoing", AiringStatus.Ongoing)]
    [InlineData("Finished", AiringStatus.Finished)]
    [InlineData("Delayed", AiringStatus.Delayed)]
    public async Task Provider_Status_Maps_To_AiringStatus(string providerStatus, AiringStatus expected)
    {
        var client = ClientReturning(CreateAnime(title: "X", status: providerStatus));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(expected, Single(data).Airing));
    }

    [Theory]
    [InlineData("ONGOING", AiringStatus.Ongoing)]
    [InlineData("finished", AiringStatus.Finished)]
    public async Task Provider_Status_Casing_Is_Ignored(string providerStatus, AiringStatus expected)
    {
        var client = ClientReturning(CreateAnime(title: "X", status: providerStatus));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(expected, Single(data).Airing));
    }

    // The provider's documented value set already omits a value it sends, so unrecognised input
    // must degrade rather than throw.
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Hiatus")]
    public async Task Unrecognised_Provider_Status_Is_Unknown(string providerStatus)
    {
        var client = ClientReturning(CreateAnime(title: "X", status: providerStatus));

        var result = await EmptyFeed().ScrapSeries(client, Spring2026, CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(AiringStatus.Unknown, Single(data).Airing));
    }

    #endregion

    #region Test Helpers

    private static Task<Result<ImmutableArray<FeedData>>> EmptyFeed() =>
        Task.FromResult(Result<ImmutableArray<FeedData>>.Success([]));

    private static StorageData Single(ScrapTvLibraryData data) => data.SeriesData.Single();

    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));

    private static IAnimeScheduleClient ClientReturning(params AnimeScheduleAnime[] series)
    {
        var client = Substitute.For<IAnimeScheduleClient>();
        client.GetSeason(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ImmutableArray<AnimeScheduleAnime>>.Success([..series])));
        return client;
    }

    private static AnimeScheduleAnime CreateAnime(
        string id = "id",
        string title = "Title",
        string? description = "A description",
        string? imageVersionRoute = "anime/jpg/default/show.jpg",
        DateTime? premier = null,
        string mediaTypeRoute = "tv",
        AnimeScheduleMediaType[]? mediaTypes = null,
        string status = "Ongoing") =>
        new(id,
            title,
            description,
            imageVersionRoute,
            premier,
            new AnimeScheduleSeason("Summer 2026", "2026", "Summer", "summer-2026"),
            Names: null,
            mediaTypes ?? [new AnimeScheduleMediaType(mediaTypeRoute.ToUpperInvariant(), mediaTypeRoute)],
            status);

    #endregion
}
