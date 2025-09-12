using System.Collections.Immutable;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess
{
    public class ScrapStepsTests
    {
        private readonly IFixture _defaultFixture = new Fixture()
            .Customize(new DefaultSeasonDataCustomization())
            .Customize(new AutoFakeItEasyCustomization());

        [Fact]
        public async Task Should_Enrich_Data_With_StoredSeries()
        {
            // Create initial ScrapTvLibraryData
            var seriesSeason = _defaultFixture
                .Create<SeriesSeason>();

            var feedTitles = ImmutableList.Create("Series 1", "Test Anime");

            var storageData = new StorageData(
                new AnimeInfoStorage
                {
                    RowKey = "1",
                    PartitionKey = "season-2025",
                    Title = "Test Anime",
                    Synopsis = "Test synopsis",
                    FeedTitle = string.Empty,
                    Status = SeriesStatus.NotAvailableValue
                },
                new NoImage());

            var seriesDataList = ImmutableList.Create(storageData);
            var scrapData = new ScrapTvLibraryData(seriesDataList, feedTitles, seriesSeason);
            var resultScrapData = Result<ScrapTvLibraryData>.Success(scrapData);
            var initialData = Task.FromResult(resultScrapData);

            // Create stored series
            var storedSeries = ImmutableList.Create(
                new TvSeriesInfo(
                    "Test Anime",
                    "Test Anime Feed",
                    ["Alt Title 1", "Alt Title 2"],
                    SeriesStatus.Ongoing()));
                    SeriesStatus.Ongoing();

            // Setup StoredSeriesGetter fake
            var storedSeriesGetter = A.Fake<TableStorageStoredSeries>();
            A.CallTo(() => storedSeriesGetter(seriesSeason, A<CancellationToken>._))
                .Returns(Task.FromResult(Result<ImmutableList<TvSeriesInfo>>.Success(storedSeries)));

            // Setup TimeProvider fake
            var timeProvider = A.Fake<TimeProvider>();
            A.CallTo(() => timeProvider.GetUtcNow())
                .Returns(DateTimeOffset.Parse("2025-05-05T14:30:45+04:00"));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, timeProvider, CancellationToken.None);

            Assert.True(result.IsSuccess);
            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal("Test Anime Feed", updatedSeries.FeedTitle);
                Assert.Equal(SeriesStatus.OngoingValue, updatedSeries.Status);
                Assert.Equal("Alt Title 1|Alt Title 2", updatedSeries.AlternativeTitles);
            });
        }

        [Fact]
        public async Task Should_Set_Status_To_Ongoing_When_NewSeries_Have_Matching_Feed()
        {
            // Create initial ScrapTvLibraryData
            var seriesSeason = _defaultFixture
                .Create<SeriesSeason>();
            var feedTitles = ImmutableList.Create("Test Anime", "Series 2"); // Matching feed title

            var storageData = new StorageData(
                new AnimeInfoStorage
                {
                    RowKey = "1",
                    PartitionKey = "season-2025",
                    Title = "Test Anime",
                    Synopsis = "Test synopsis",
                    FeedTitle = string.Empty,
                    Status = SeriesStatus.NotAvailableValue
                },
                new NoImage());

            var seriesDataList = ImmutableList.Create(storageData);
            var scrapData = new ScrapTvLibraryData(seriesDataList, feedTitles, seriesSeason);
            var resultScrapData = Result<ScrapTvLibraryData>.Success(scrapData);
            var initialData = Task.FromResult(resultScrapData);

            // Empty stored series (no existing series)
            var storedSeries = ImmutableList<TvSeriesInfo>.Empty;

            // Setup StoredSeriesGetter fake
            var storedSeriesGetter = A.Fake<TableStorageStoredSeries>();
            A.CallTo(() => storedSeriesGetter(seriesSeason, A<CancellationToken>._))
                .Returns(Task.FromResult(Result<ImmutableList<TvSeriesInfo>>.Success(storedSeries)));

            // Setup TimeProvider fake - current season
            var timeProvider = A.Fake<TimeProvider>();
            A.CallTo(() => timeProvider.GetUtcNow())
                .Returns(DateTimeOffset.Parse("2025-05-05T14:30:45+04:00"));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, timeProvider, CancellationToken.None);

            Assert.True(result.IsSuccess);
            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal("Test Anime", updatedSeries.FeedTitle);
                Assert.Equal(SeriesStatus.OngoingValue, updatedSeries.Status);
            });
        }


        [Fact]
        internal async Task Should_Set_Status_Completed_When_New_And_Is_OldSeason_And_NoMatchingFeed()
        {
            await OldSeasonVerification(ImmutableList<TvSeriesInfo>.Empty);
        }

        [Fact]
        internal async Task Should_Set_Status_Completed_When_Exist_And_Is_OldSeason_And_NoMatchingFeed()
        {
            var storedSeries = new TvSeriesInfo("Test Anime", string.Empty, [], SeriesStatus.NotAvailable());

            await OldSeasonVerification(ImmutableList.Create(storedSeries));
        }

        private async Task OldSeasonVerification(ImmutableList<TvSeriesInfo> dbSeries)
        {
            // Create initial ScrapTvLibraryData
            // Season to scrap will be old
            var seriesSeason = new SeriesSeason(Season.Summer(), Year.FromNumber(2024));
            var feedTitles = ImmutableList.Create("Other Series"); // No matching feed title

            var processSeries = new StorageData(new AnimeInfoStorage
            {
                RowKey = "1",
                PartitionKey = "2023-spring", // Old season
                Title = "Test Anime",
                Synopsis = "Test synopsis",
                FeedTitle = string.Empty,
                Status = SeriesStatus.NotAvailableValue
            }, new NoImage());

            var scrapData = new ScrapTvLibraryData(ImmutableList.Create(processSeries), feedTitles, seriesSeason);
            var resultScrapData = Result<ScrapTvLibraryData>.Success(scrapData);
            var initialData = Task.FromResult(resultScrapData);

            // Empty stored series (no existing series)
            var storedSeries = dbSeries;

            // Setup StoredSeriesGetter fake
            var storedSeriesGetter = A.Fake<TableStorageStoredSeries>();
            A.CallTo(() => storedSeriesGetter(seriesSeason, A<CancellationToken>._))
                .Returns(Task.FromResult(Result<ImmutableList<TvSeriesInfo>>.Success(storedSeries)));

            // Setup TimeProvider fake with a date in the future against the scrapped seasoon
            var timeProvider = A.Fake<TimeProvider>();
            A.CallTo(() => timeProvider.GetUtcNow())
                .Returns(DateTimeOffset.Parse("2025-05-05T14:30:45+04:00"));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, timeProvider, CancellationToken.None);

            Assert.True(result.IsSuccess);
            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal(string.Empty, updatedSeries.FeedTitle);
                Assert.Equal(SeriesStatus.CompletedValue, updatedSeries.Status);
            });
        }
    }
}