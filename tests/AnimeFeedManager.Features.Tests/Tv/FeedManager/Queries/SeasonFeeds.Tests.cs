using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Queries;

public class SeasonFeedsTests
{
    private static readonly SeriesSeason Fall = new(Season.Fall(), Year.FromNumber(2026));
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 6, 0, 0, TimeSpan.Zero);

    #region Unfed Tests

    [Fact]
    public async Task Should_List_Series_As_Unfed_When_It_Has_A_Feed_Title_But_No_Binding()
    {
        var series = MakeSeries("2026_fall_psyren", "Psyren", SeriesStatus.Ongoing());

        var result = await GetSeasonFeeds([series], []);

        result.AssertOnSuccess(feeds =>
            Assert.Equal("2026_fall_psyren", Assert.IsType<UnfedSeries>(Assert.Single(feeds)).SeriesId));
    }

    [Fact]
    public async Task Should_List_Series_Only_As_Bound_When_It_Has_A_Binding()
    {
        var series = MakeSeries("2026_fall_psyren", "Psyren", SeriesStatus.Ongoing());

        var result = await GetSeasonFeeds([series], [MakeBinding("2026_fall_psyren")]);

        result.AssertOnSuccess(feeds =>
            Assert.Equal("2026_fall_psyren", Assert.IsType<BoundSeries>(Assert.Single(feeds)).SeriesId));
    }

    [Fact]
    public async Task Should_Exclude_Series_When_It_Is_Completed_And_Unbound()
    {
        var series = MakeSeries("2026_fall_psyren", "Psyren", SeriesStatus.Completed());

        var result = await GetSeasonFeeds([series], []);

        result.AssertOnSuccess(feeds => Assert.Empty(feeds));
    }

    #endregion

    #region Test Helpers

    private static Task<Result<ImmutableArray<SeasonFeed>, DomainError>> GetSeasonFeeds(
        ImmutableArray<TvSeries> library,
        ImmutableArray<NyaaBindingStorage> bindings) =>
        SeasonFeeds.GetSeasonFeeds(
            Fall,
            new Uri("https://blob.example.com"),
            (_, _, _) => Task.FromResult(Result<ImmutableArray<TvSeries>, DomainError>.Success(library)),
            (_, _) => Task.FromResult(Result<ImmutableArray<NyaaBindingStorage>, DomainError>.Success(bindings)),
            (_, _) => Task.FromResult(Result<ImmutableArray<NyaaCatalogStorage>, DomainError>.Success([])),
            Now,
            CancellationToken.None);

    private static TvSeries MakeSeries(string id, string feedTitle, SeriesStatus status) => new(
        id,
        Fall,
        feedTitle,
        "synopsis",
        feedTitle,
        AlternativeTitlesData.Empty,
        status,
        null,
        null,
        new UnknownEpisodes(),
        new BroadcastHint(new UnknownRelease(), []));

    private static NyaaBindingStorage MakeBinding(string seriesId) => new()
    {
        PartitionKey = "2026-fall",
        RowKey = seriesId,
        Group = "SubsPlease",
        FeedTitle = "Psyren",
        Tracking = true
    };

    #endregion
}
