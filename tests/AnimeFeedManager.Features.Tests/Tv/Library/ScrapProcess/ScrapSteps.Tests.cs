using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess;

public class ScrapStepsTests
{
    private const string New = "";

    #region Stored Data Tests

    [Fact]
    public async Task Should_Carry_Stored_User_Titles_When_Series_Exists()
    {
        var stored = Stored(SeriesStatus.NotAvailable(), new AlternativeTitlesData(User: ["Alt Title 1", "Alt Title 2"]));

        var result = await Enrich([stored], AiringStatus.Unknown);

        result.AssertOnSuccess(r => Assert.Equal(["Alt Title 1", "Alt Title 2"],
            StoredAlternativeTitles.Parse(r.SeriesData.Single().Series.AlternativeTitles).User!));
    }

    [Fact]
    public async Task Should_Leave_Feed_Fields_Unset_When_Stored_Series_Has_A_Feed()
    {
        var stored = new TvSeriesInfo("Test Anime", "Test Anime Feed", "https://example.com/feed",
            AlternativeTitlesData.Empty, SeriesStatus.Ongoing());

        var result = await Enrich([stored], AiringStatus.Ongoing);

        result.AssertOnSuccess(r =>
        {
            var series = r.SeriesData.Single().Series;
            Assert.Null(series.FeedTitle);
            Assert.Null(series.FeedLink);
            Assert.Null(series.FeedSource);
        });
    }

    #endregion

    #region Status Tests

    [Theory]
    [InlineData(New, AiringStatus.Finished, SeriesStatus.CompletedValue)]
    [InlineData(New, AiringStatus.Ongoing, SeriesStatus.NotAvailableValue)]
    [InlineData(New, AiringStatus.Upcoming, SeriesStatus.NotAvailableValue)]
    [InlineData(New, AiringStatus.Unknown, SeriesStatus.NotAvailableValue)]
    [InlineData(SeriesStatus.NotAvailableValue, AiringStatus.Finished, SeriesStatus.CompletedValue)]
    [InlineData(SeriesStatus.NotAvailableValue, AiringStatus.Ongoing, SeriesStatus.NotAvailableValue)]
    [InlineData(SeriesStatus.OngoingValue, AiringStatus.Finished, SeriesStatus.OngoingValue)]
    [InlineData(SeriesStatus.OngoingValue, AiringStatus.Delayed, SeriesStatus.OngoingValue)]
    [InlineData(SeriesStatus.CompletedValue, AiringStatus.Unknown, SeriesStatus.CompletedValue)]
    [InlineData(SeriesStatus.CompletedValue, AiringStatus.Ongoing, SeriesStatus.CompletedValue)]
    public async Task Should_Resolve_Status_From_Stored_Status_And_Airing_When_Imported(
        string storedStatus, AiringStatus airing, string expected)
    {
        ImmutableArray<TvSeriesInfo> stored = storedStatus == New ? [] : [Stored(ToStatus(storedStatus))];

        var result = await Enrich(stored, airing);

        result.AssertOnSuccess(r => Assert.Equal(expected, r.SeriesData.Single().Series.Status));
    }

    [Fact]
    public async Task Should_Mark_Updated_When_Stored_Series_Status_Changes()
    {
        var result = await Enrich([Stored(SeriesStatus.NotAvailable())], AiringStatus.Finished);

        result.AssertOnSuccess(r => Assert.Equal(Status.UpdatedSeries, r.SeriesData.Single().Status));
    }

    [Fact]
    public async Task Should_Mark_No_Changes_When_Stored_Series_Status_Holds()
    {
        var result = await Enrich([Stored(SeriesStatus.Ongoing())], AiringStatus.Ongoing);

        result.AssertOnSuccess(r => Assert.Equal(Status.NoChanges, r.SeriesData.Single().Status));
    }

    [Fact]
    public async Task Should_Mark_New_When_Series_Is_Not_Stored()
    {
        var result = await Enrich([], AiringStatus.Ongoing);

        result.AssertOnSuccess(r => Assert.Equal(Status.NewSeries, r.SeriesData.Single().Status));
    }

    #endregion

    #region Test Helpers

    private static TvSeriesInfo Stored(SeriesStatus status, AlternativeTitlesData? titles = null) =>
        new("Test Anime", string.Empty, null, titles ?? AlternativeTitlesData.Empty, status);

    private static SeriesStatus ToStatus(string status) => status switch
    {
        SeriesStatus.OngoingValue => SeriesStatus.Ongoing(),
        SeriesStatus.CompletedValue => SeriesStatus.Completed(),
        _ => SeriesStatus.NotAvailable()
    };

    // Mirrors what the provider scrape hands to enrichment: feed fields unset, status NotAvailable.
    private static Task<Result<ScrapTvLibraryData>> Enrich(ImmutableArray<TvSeriesInfo> stored, AiringStatus airing)
    {
        var season = TestSeasons.Default;
        var imported = new StorageData(new AnimeInfoStorage
        {
            RowKey = "1",
            PartitionKey = "2024-summer",
            Title = "Test Anime",
            Synopsis = "Test synopsis",
            Status = SeriesStatus.NotAvailableValue
        }, new NoImage(), Status.NewSeries, airing);

        var storedSeriesGetter = Substitute.For<StoredSeriesGetter>();
        storedSeriesGetter(season, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ImmutableArray<TvSeriesInfo>>.Success(stored)));

        return Task.FromResult(Result<ScrapTvLibraryData>.Success(new ScrapTvLibraryData([imported], season)))
            .AddDataFromStorage(storedSeriesGetter, CancellationToken.None);
    }

    #endregion
}
