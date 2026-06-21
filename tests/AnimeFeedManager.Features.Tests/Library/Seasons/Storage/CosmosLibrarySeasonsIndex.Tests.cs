using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Tests.Library.Seasons.Storage;

public class CosmosLibrarySeasonsIndexTests
{
    [Fact]
    public void Should_Return_Latest_Season_When_Entries_Span_Multiple_Years()
    {
        var index = IndexWith(
            SeasonOf(2025, Season.Summer()),
            SeasonOf(2026, Season.Winter()),
            SeasonOf(2024, Season.Fall()));

        Assert.Equal(SeasonOf(2026, Season.Winter()), Resolve(index));
    }

    [Fact]
    public void Should_Return_Latest_Season_When_Entries_Share_A_Year()
    {
        var index = IndexWith(
            SeasonOf(2026, Season.Winter()),
            SeasonOf(2026, Season.Fall()),
            SeasonOf(2026, Season.Spring()));

        Assert.Equal(SeasonOf(2026, Season.Fall()), Resolve(index));
    }

    [Fact]
    public void Should_Return_The_Only_Season_When_Index_Has_One_Entry()
    {
        var index = IndexWith(SeasonOf(2026, Season.Spring()));

        Assert.Equal(SeasonOf(2026, Season.Spring()), Resolve(index));
    }

    [Fact]
    public void Should_Fail_With_NotFoundError_When_Index_Is_Empty()
    {
        Assert.IsType<NotFoundError>(ResolveError(new LibrarySeasonsIndex()));
    }

    [Fact]
    public void Should_Fail_With_NotFoundError_When_Seasons_Array_Is_Default()
    {
        Assert.IsType<NotFoundError>(ResolveError(new LibrarySeasonsIndex { Seasons = default }));
    }

    private static SeriesSeason SeasonOf(int year, Season season) => new(season, Year.FromNumber(year));

    private static LibrarySeasonsIndex IndexWith(params SeriesSeason[] seasons) =>
        new() { Seasons = [.. seasons.Select(s => new SeasonEntry(s, DateTimeOffset.UnixEpoch, SeriesCount: 0))] };

    private static SeriesSeason? Resolve(LibrarySeasonsIndex index) =>
        CosmosLibrarySeasonsIndex.ResolveLatest(index).MatchToValue(season => (SeriesSeason?)season, _ => null);

    private static DomainError? ResolveError(LibrarySeasonsIndex index) =>
        CosmosLibrarySeasonsIndex.ResolveLatest(index).MatchToValue(_ => (DomainError?)null, error => error);
}
