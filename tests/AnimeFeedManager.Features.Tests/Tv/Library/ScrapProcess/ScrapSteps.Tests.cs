using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess
{
    public class ScrapStepsTests
    {
        [Fact]
        public async Task Should_Enrich_Data_With_StoredSeries()
        {
            // Create initial ScrapTvLibraryData
            var seriesSeason = TestSeasons.Default;

            var feedTitles = ImmutableArray.Create(
                new FeedData("Series 1", "https://example.com/series-1"),
                new FeedData("Test Anime", "https://example.com/test-anime"));

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
                new NoImage(),
                Status.NewSeries,
                AiringStatus.Unknown);

            var seriesDataList = ImmutableArray.Create(storageData);
            var scrapData = new ScrapTvLibraryData(seriesDataList, feedTitles, seriesSeason);
            var resultScrapData = Result<ScrapTvLibraryData>.Success(scrapData);
            var initialData = Task.FromResult(resultScrapData);

            // Create stored series
            var storedSeries = ImmutableArray.Create(
                new TvSeriesInfo(
                    "Test Anime",
                    "Test Anime Feed",
                    "https://example.com/test-anime-feed",
                    new AlternativeTitlesData(User: ["Alt Title 1", "Alt Title 2"]),
                    SeriesStatus.Ongoing()));
            
            // Setup StoredSeriesGetter fake
            var storedSeriesGetter = Substitute.For<StoredSeriesGetter>();
            storedSeriesGetter(seriesSeason, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<ImmutableArray<TvSeriesInfo>>.Success(storedSeries)));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, CancellationToken.None);

            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal("Test Anime Feed", updatedSeries.FeedTitle);
                Assert.Equal("https://example.com/test-anime-feed", updatedSeries.FeedLink);
                Assert.Equal(SeriesStatus.OngoingValue, updatedSeries.Status);
                Assert.Equal(["Alt Title 1", "Alt Title 2"],
                    StoredAlternativeTitles.Parse(updatedSeries.AlternativeTitles).User!);
            });
        }

        [Fact]
        public async Task Should_Set_Status_To_Ongoing_When_NewSeries_Have_Matching_Feed()
        {
            // Create initial ScrapTvLibraryData
            var seriesSeason = TestSeasons.Default;
            var feedTitles = ImmutableArray.Create(
                new FeedData("Test Anime", "https://example.com/test-anime"),
                new FeedData("Series 2", "https://example.com/series-2")); // Matching feed title

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
                new NoImage(),
                Status.NewSeries,
                AiringStatus.Unknown);

            var seriesDataList = ImmutableArray.Create(storageData);
            var scrapData = new ScrapTvLibraryData(seriesDataList, feedTitles, seriesSeason);
            var resultScrapData = Result<ScrapTvLibraryData>.Success(scrapData);
            var initialData = Task.FromResult(resultScrapData);

            // Empty stored series (no existing series)
            var storedSeries = ImmutableArray<TvSeriesInfo>.Empty;

            // Setup StoredSeriesGetter fake
            var storedSeriesGetter = Substitute.For<StoredSeriesGetter>();
            storedSeriesGetter(seriesSeason, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<ImmutableArray<TvSeriesInfo>>.Success(storedSeries)));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, CancellationToken.None);

            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal("Test Anime", updatedSeries.FeedTitle);
                Assert.Equal("https://example.com/test-anime", updatedSeries.FeedLink);
                Assert.Equal(SeriesStatus.OngoingValue, updatedSeries.Status);
            });
        }

        [Theory]
        [InlineData(AiringStatus.Ongoing)]
        [InlineData(AiringStatus.Upcoming)]
        [InlineData(AiringStatus.Finished)]
        [InlineData(AiringStatus.Delayed)]
        [InlineData(AiringStatus.Unknown)]
        public async Task Should_Set_Status_Ongoing_When_Feed_Matches_Whatever_The_Provider_Says(AiringStatus airing)
        {
            await FeedMatchVerification(ImmutableArray<TvSeriesInfo>.Empty, airing, SeriesStatus.OngoingValue);
        }

        [Fact]
        public async Task Should_Reopen_A_Completed_Series_When_It_Returns_To_The_Feed()
        {
            var storedSeries = new TvSeriesInfo("Test Anime", string.Empty, null, AlternativeTitlesData.Empty,
                SeriesStatus.Completed());
            await FeedMatchVerification([storedSeries], AiringStatus.Finished, SeriesStatus.OngoingValue);
        }

        private async Task FeedMatchVerification(
            ImmutableArray<TvSeriesInfo> dbSeries,
            AiringStatus airing,
            string expectedStatus)
        {
            var seriesSeason = TestSeasons.Default;
            var feedTitles = ImmutableArray.Create(
                new FeedData("Test Anime", "https://example.com/test-anime"));

            var processSeries = new StorageData(new AnimeInfoStorage
            {
                RowKey = "1",
                PartitionKey = "2024-summer",
                Title = "Test Anime",
                Synopsis = "Test synopsis",
                FeedTitle = string.Empty,
                Status = SeriesStatus.NotAvailableValue
            }, new NoImage(), Status.NewSeries, airing);

            var scrapData = new ScrapTvLibraryData([processSeries], feedTitles, seriesSeason);
            var initialData = Task.FromResult(Result<ScrapTvLibraryData>.Success(scrapData));

            var storedSeriesGetter = Substitute.For<StoredSeriesGetter>();
            storedSeriesGetter(seriesSeason, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<ImmutableArray<TvSeriesInfo>>.Success(dbSeries)));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, CancellationToken.None);

            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal("Test Anime", updatedSeries.FeedTitle);
                Assert.Equal(expectedStatus, updatedSeries.Status);
            });
        }

        [Fact]
        internal async Task Should_Set_Status_Completed_When_New_And_Provider_Says_Finished_And_NoMatchingFeed()
        {
            await NoFeedMatchVerification(ImmutableArray<TvSeriesInfo>.Empty, AiringStatus.Finished,
                SeriesStatus.CompletedValue);
        }

        [Fact]
        internal async Task Should_Set_Status_Completed_When_Exist_And_Provider_Says_Finished_And_NoMatchingFeed()
        {
            var storedSeries = new TvSeriesInfo("Test Anime", string.Empty, null, AlternativeTitlesData.Empty,
                SeriesStatus.NotAvailable());
            await NoFeedMatchVerification([storedSeries], AiringStatus.Finished, SeriesStatus.CompletedValue);
        }

        [Fact]
        internal async Task Should_Set_Status_NotAvailable_When_New_And_Provider_Status_Is_Not_Finished()
        {
            await NoFeedMatchVerification(ImmutableArray<TvSeriesInfo>.Empty, AiringStatus.Upcoming,
                SeriesStatus.NotAvailableValue);
        }

        [Fact]
        internal async Task Should_Hold_Ongoing_When_Series_Leaves_The_Feed_And_Provider_Has_Not_Finished_It()
        {
            var storedSeries = new TvSeriesInfo("Test Anime", string.Empty, null, AlternativeTitlesData.Empty,
                SeriesStatus.Ongoing());
            await NoFeedMatchVerification([storedSeries], AiringStatus.Delayed, SeriesStatus.OngoingValue);
        }

        [Fact]
        internal async Task Should_Hold_Completed_When_A_Completed_Series_Is_Reimported()
        {
            var storedSeries = new TvSeriesInfo("Test Anime", string.Empty, null, AlternativeTitlesData.Empty,
                SeriesStatus.Completed());
            await NoFeedMatchVerification([storedSeries], AiringStatus.Unknown, SeriesStatus.CompletedValue);
        }

        private async Task NoFeedMatchVerification(
            ImmutableArray<TvSeriesInfo> dbSeries,
            AiringStatus airing,
            string expectedStatus)
        {
            var seriesSeason = TestSeasons.Default;
            var feedTitles = ImmutableArray.Create(
                new FeedData("Other Series", "https://example.com/other-series")); // No matching feed title

            var processSeries = new StorageData(new AnimeInfoStorage
            {
                RowKey = "1",
                PartitionKey = "2024-summer",
                Title = "Test Anime",
                Synopsis = "Test synopsis",
                FeedTitle = string.Empty,
                Status = SeriesStatus.NotAvailableValue
            }, new NoImage(), Status.NewSeries, airing);

            var scrapData = new ScrapTvLibraryData([processSeries], feedTitles, seriesSeason);
            var initialData = Task.FromResult(Result<ScrapTvLibraryData>.Success(scrapData));

            var storedSeriesGetter = Substitute.For<StoredSeriesGetter>();
            storedSeriesGetter(seriesSeason, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<ImmutableArray<TvSeriesInfo>>.Success(dbSeries)));

            var result = await initialData.AddDataFromStorage(storedSeriesGetter, CancellationToken.None);

            result.AssertOnSuccess(r =>
            {
                var updatedSeries = r.SeriesData.First().Series;
                Assert.Equal(string.Empty, updatedSeries.FeedTitle);
                Assert.Equal(expectedStatus, updatedSeries.Status);
            });
        }
    }
}