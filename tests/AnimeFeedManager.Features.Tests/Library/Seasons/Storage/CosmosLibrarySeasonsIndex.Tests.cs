using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Tests.Library.Seasons.Storage;

public class CosmosLibrarySeasonsIndexTests
{
    private static readonly SeriesSeason Summer2025 = SeasonOf(2025, Season.Summer());
    private static readonly SeriesSeason Fall2025 = SeasonOf(2025, Season.Fall());
    private static readonly SeriesSeason Winter2026 = SeasonOf(2026, Season.Winter());
    private static readonly SeriesSeason Summer2026 = SeasonOf(2026, Season.Summer());
    private static readonly SeriesSeason Fall2026 = SeasonOf(2026, Season.Fall());

    #region ResolveLatest

    [Fact]
    public void Should_Return_Latest_Season_When_Entries_Span_Multiple_Years()
    {
        var index = IndexWith(Summer2025, Winter2026, SeasonOf(2024, Season.Fall()));

        Assert.Equal(Winter2026, Resolve(index));
    }

    [Fact]
    public void Should_Return_Latest_Season_When_Entries_Share_A_Year()
    {
        var index = IndexWith(Winter2026, Fall2026, SeasonOf(2026, Season.Spring()));

        Assert.Equal(Fall2026, Resolve(index));
    }

    [Fact]
    public void Should_Return_The_Only_Season_When_Index_Has_One_Entry()
    {
        var index = IndexWith(SeasonOf(2026, Season.Spring()));

        Assert.Equal(SeasonOf(2026, Season.Spring()), Resolve(index));
    }

    [Fact]
    public void Should_Prefer_The_Current_Marker_Over_The_Calendar_Latest()
    {
        var index = IndexOf(Entry(Fall2026), Entry(Summer2026, isCurrent: true));

        Assert.Equal(Summer2026, Resolve(index));
    }

    [Fact]
    public void Should_Fall_Back_To_Calendar_Latest_When_No_Entry_Is_Marked_Current()
    {
        var index = IndexOf(Entry(Fall2025), Entry(Winter2026));

        Assert.Equal(Winter2026, Resolve(index));
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

    #endregion

    #region Merge — current import

    [Fact]
    public void Should_Mark_Incoming_Current_And_Clear_Others_On_Current_Import()
    {
        var existing = ImmutableArray.Create(Entry(Summer2025, isCurrent: true), Entry(Fall2025));

        var result = CosmosLibrarySeasonsIndex.Merge(existing, Entry(Winter2026), SeasonImportKind.Current);

        Assert.Equal(Winter2026, Assert.Single(result, e => e.IsCurrent).SeriesSeason);
    }

    [Fact]
    public void Should_Stay_Current_When_Current_Import_Reimports_The_Same_Season()
    {
        var existing = ImmutableArray.Create(Entry(Summer2026, isCurrent: true));

        var result = CosmosLibrarySeasonsIndex.Merge(existing, Entry(Summer2026), SeasonImportKind.Current);

        Assert.True(Assert.Single(result).IsCurrent);
    }

    [Fact]
    public void Should_Replace_Promote_And_Demote_When_Current_Import_Targets_An_Already_Present_Season()
    {
        // Fall 2026 was imported earlier as a future preview (not current); Summer 2026 is airing.
        var existing = ImmutableArray.Create(Entry(Summer2026, isCurrent: true), Entry(Fall2026));
        var incoming = Entry(Fall2026) with { SeriesCount = 12 };

        var result = CosmosLibrarySeasonsIndex.Merge(existing, incoming, SeasonImportKind.Current);

        Assert.Equal(2, result.Length); // no duplicate Fall 2026
        Assert.Equal(Fall2026, Assert.Single(result, e => e.IsCurrent).SeriesSeason);
        Assert.False(result.Single(e => e.SeriesSeason == Summer2026).IsCurrent); // prior current demoted
        Assert.Equal(12, result.Single(e => e.SeriesSeason == Fall2026).SeriesCount); // refreshed
    }

    #endregion

    #region Merge — specific import

    [Fact]
    public void Should_Preserve_The_Current_Marker_When_A_Specific_Import_Reimports_The_Airing_Season()
    {
        var existing = ImmutableArray.Create(Entry(Summer2026, isCurrent: true));
        var incoming = Entry(Summer2026) with { SeriesCount = 30 };

        var result = CosmosLibrarySeasonsIndex.Merge(existing, incoming, SeasonImportKind.Specific);

        Assert.True(Assert.Single(result).IsCurrent); // not demoted
        Assert.Equal(30, Assert.Single(result).SeriesCount); // refreshed
    }

    [Fact]
    public void Should_Add_As_Non_Current_And_Leave_The_Marker_When_A_Specific_Import_Targets_A_New_Season()
    {
        var existing = ImmutableArray.Create(Entry(Summer2026, isCurrent: true));

        var result = CosmosLibrarySeasonsIndex.Merge(existing, Entry(Fall2025), SeasonImportKind.Specific);

        Assert.Equal(2, result.Length);
        Assert.Equal(Summer2026, Assert.Single(result, e => e.IsCurrent).SeriesSeason); // unchanged
        Assert.False(result.Single(e => e.SeriesSeason == Fall2025).IsCurrent);
    }

    #endregion

    #region Test Helpers

    private static SeriesSeason SeasonOf(int year, Season season) => new(season, Year.FromNumber(year));

    private static SeasonEntry Entry(SeriesSeason season, bool isCurrent = false) =>
        new(season, DateTimeOffset.UnixEpoch, SeriesCount: 0, RepresentativePoster: "poster.webp", IsCurrent: isCurrent);

    private static LibrarySeasonsIndex IndexWith(params SeriesSeason[] seasons) =>
        new() { Seasons = [.. seasons.Select(s => Entry(s))] };

    private static LibrarySeasonsIndex IndexOf(params SeasonEntry[] entries) =>
        new() { Seasons = [.. entries] };

    private static SeriesSeason? Resolve(LibrarySeasonsIndex index) =>
        CosmosLibrarySeasonsIndex.ResolveLatest(index).MatchToValue(season => (SeriesSeason?)season, _ => null);

    private static DomainError? ResolveError(LibrarySeasonsIndex index) =>
        CosmosLibrarySeasonsIndex.ResolveLatest(index).MatchToValue(_ => (DomainError?)null, error => error);

    #endregion
}
