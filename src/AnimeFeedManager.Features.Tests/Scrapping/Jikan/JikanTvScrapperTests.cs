using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Jikan;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tests.Scrapping.Jikan;

public class JikanTvScrapperTests
{
    [Fact]
    public async Task Latest_Routes_To_GetCurrentSeason()
    {
        var anime = CreateAnime(title: "X", season: "spring", year: 2026);
        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success([anime])));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => jikanClient.GetSeason(A<int>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        result.AssertOnSuccess(data => Assert.Single(data.SeriesData));
    }

    [Fact]
    public async Task BySeason_Routes_To_GetSeason_With_Args()
    {
        var anime = CreateAnime(title: "X", season: "spring", year: 2026);
        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetSeason(2026, "spring", A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success([anime])));

        var selector = new BySeason(Season.Spring(), Year.FromNumber(2026));
        var result = await EmptyFeed().ScrapSeries(jikanClient, selector, CancellationToken.None);

        A.CallTo(() => jikanClient.GetSeason(2026, "spring", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._)).MustNotHaveHappened();
        result.AssertOnSuccess(data => Assert.Single(data.SeriesData));
    }

    [Fact]
    public async Task Non_TV_Items_Filtered_Out()
    {
        var tv = CreateAnime(title: "TVShow", type: "TV");
        var ona = CreateAnime(title: "OnaShow", type: "ONA");
        var movie = CreateAnime(title: "MovieShow", type: "Movie");

        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success(
                [tv, ona, movie])));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var entries = data.SeriesData.ToImmutableArray();
            Assert.Single(entries);
            Assert.Equal("TVShow", entries[0].Series.Title);
        });
    }

    [Fact]
    public async Task Mapping_Produces_Correct_Storage_Data()
    {
        var airedDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var anime = CreateAnime(
            title: "X",
            synopsis: "Y",
            imageUrl: "https://example.test/x.jpg",
            airedFrom: airedDate,
            season: "spring",
            year: 2026);

        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success([anime])));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        result.AssertOnSuccess(data =>
        {
            var entry = data.SeriesData.First();
            var storage = entry.Series;

            Assert.Equal("X", storage.Title);
            Assert.Equal("Y", storage.Synopsis);
            Assert.Equal(airedDate, storage.Date);
            Assert.Equal(IdHelpers.GenerateAnimeId("spring", "2026", "X"), storage.RowKey);
            Assert.Equal(IdHelpers.GenerateAnimePartitionKey("spring", 2026), storage.PartitionKey);
            Assert.Null(storage.FeedTitle);
            Assert.Null(storage.FeedLink);
            Assert.Equal(SeriesStatus.NotAvailableValue, storage.Status);

            Assert.Equal(Status.NewSeries, entry.Status);
            var image = Assert.IsType<ScrappedImageUrl>(entry.Image);
            Assert.Equal("https://example.test/x.jpg", image.Url.ToString());
        });
    }

    [Fact]
    public async Task Null_Synopsis_Becomes_Empty_String()
    {
        var anime = CreateAnime(synopsis: null, season: "spring", year: 2026);
        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success([anime])));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        result.AssertOnSuccess(data => Assert.Equal(string.Empty, data.SeriesData.First().Series.Synopsis));
    }

    [Fact]
    public async Task Invalid_Image_Url_Becomes_NoImage()
    {
        var anime = CreateAnime(imageUrl: null, season: "spring", year: 2026);
        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Success([anime])));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        result.AssertOnSuccess(data => Assert.IsType<NoImage>(data.SeriesData.First().Image));
    }

    [Fact]
    public async Task Client_Failure_Propagates_As_Result_Failure()
    {
        var jikanClient = A.Fake<IJikanClient>();
        A.CallTo(() => jikanClient.GetCurrentSeason(A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableArray<JikanAnime>>.Failure(HandledError.Create())));

        var result = await EmptyFeed().ScrapSeries(jikanClient, new Latest(), CancellationToken.None);

        result.AssertError();
    }

    private static Task<Result<ImmutableArray<FeedData>>> EmptyFeed() =>
        Task.FromResult(Result<ImmutableArray<FeedData>>.Success(ImmutableArray<FeedData>.Empty));

    private static JikanAnime CreateAnime(
        string title = "Test",
        string? synopsis = "Test synopsis",
        string? imageUrl = "https://example.test/image.jpg",
        DateTime? airedFrom = null,
        string? season = "spring",
        int? year = 2026,
        string? type = "TV",
        int malId = 0) =>
        new(
            MalId: malId,
            Title: title,
            Synopsis: synopsis,
            Images: new JikanImages(new JikanImagesJpg(imageUrl)),
            Aired: airedFrom is not null ? new JikanAired(airedFrom, "Apr 1, 2026 to ?") : null,
            Season: season,
            Year: year,
            Type: type);
}
