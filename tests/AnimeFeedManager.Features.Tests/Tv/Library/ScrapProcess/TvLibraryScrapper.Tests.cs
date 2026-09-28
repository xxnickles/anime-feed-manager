using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess;

public class TvLibraryScrapperTests
{
    #region ResolveSeason

    [Fact]
    public async Task Should_Resolve_Current_To_The_Featured_Season_When_One_Is_Flagged()
    {
        var getter = LatestReturning(new CurrentLatestSeason(StoredSeason("summer", 2026, latest: true)));

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Summer(), season.Season);
            Assert.Equal(2026, season.Year);
        });
    }

    [Fact]
    public async Task Should_Resolve_Current_To_The_Newest_Season_When_None_Is_Flagged()
    {
        var getter = LatestReturning(new FallbackLatestSeason(StoredSeason("spring", 2026)));

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Spring(), season.Season);
            Assert.Equal(2026, season.Year);
        });
    }

    [Fact]
    public async Task Should_Not_Mark_Resolved_Season_As_Latest_When_Stored_Season_Is_Featured()
    {
        var getter = LatestReturning(new CurrentLatestSeason(StoredSeason("summer", 2026, latest: true)));

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertOnSuccess(season => Assert.False(season.IsLatest));
    }

    [Fact]
    public async Task Should_Fail_When_Storage_Holds_No_Season()
    {
        var getter = LatestReturning(new NoMatch());

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task Should_Fail_When_Stored_Season_Is_Unparseable()
    {
        var getter = LatestReturning(new CurrentLatestSeason(StoredSeason("autumn-ish", 2026, latest: true)));

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task Should_Fail_When_Latest_Season_Lookup_Fails()
    {
        var getter = Substitute.For<LatestSeasonGetter>();
        getter(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<SeasonStorageData>.Failure(HandledError.Create())));

        var result = await TvLibraryScrapper.ResolveSeason(new Current(), getter, CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task Should_Use_The_Requested_Season_Without_Reading_Storage_When_Selector_Is_BySeason()
    {
        var getter = Substitute.For<LatestSeasonGetter>();

        var result = await TvLibraryScrapper.ResolveSeason(
            new BySeason(Season.Fall(), Year.FromNumber(2025)), getter, CancellationToken.None);

        _ = getter.DidNotReceive()(Arg.Any<CancellationToken>());
        result.AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Fall(), season.Season);
            Assert.Equal(2025, season.Year);
        });
    }

    #endregion

    #region Test Helpers

    private static LatestSeasonGetter LatestReturning(SeasonStorageData data)
    {
        var getter = Substitute.For<LatestSeasonGetter>();
        getter(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<SeasonStorageData>.Success(data)));
        return getter;
    }

    private static SeasonStorage StoredSeason(string season, int year, bool latest = false) =>
        new() {Season = season, Year = year, Latest = latest, RowKey = $"{year}-{season}"};

    #endregion
}
