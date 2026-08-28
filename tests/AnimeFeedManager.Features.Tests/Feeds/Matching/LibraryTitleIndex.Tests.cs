using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Matching;

public class LibraryTitleIndexTests
{
    [Fact]
    public void Should_Match_By_Default_Title()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        var matched = index.TryMatch("One Piece");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
    }

    [Fact]
    public void Should_Match_By_Any_Title_Variant()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Grow Up Show: Himawari no Circus-dan", "Grow Up Show - Himawari no Circus-dan")]);

        var matched = index.TryMatch("Grow Up Show - Himawari no Circus-dan");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
    }

    [Fact]
    public void Should_Match_Regardless_Of_Casing_And_Punctuation_Differences()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Mahou Shoujo Lyrical Nanoha Exceeds: Gun Blaze Vengeance")]);

        var matched = index.TryMatch("mahou shoujo lyrical nanoha EXCEEDS - Gun Blaze Vengeance");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
    }

    [Fact]
    public void Should_Return_Null_When_No_Series_Matches()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        Assert.Null(index.TryMatch("Naruto"));
    }

    [Fact]
    public void Should_Skip_Titles_That_Normalize_To_Nothing_Meaningful()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece", "ワンピース")]); // Japanese title, non-ASCII

        var matched = index.TryMatch("One Piece");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
        Assert.Null(index.TryMatch(""));
    }

    [Fact]
    public void Should_Build_Empty_Index_When_Library_Is_Empty()
    {
        var index = LibraryTitleIndex.Build([]);

        Assert.Null(index.TryMatch("Anything"));
    }

    [Fact]
    public void Should_Return_Canonical_Default_Title_When_Matched()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece", "ワンピース")]);

        var matched = index.TryMatch("One Piece");

        Assert.NotNull(matched);
        Assert.Equal("One Piece", matched.Title);
    }

    [Fact]
    public void Should_Return_The_Series_Season_When_Matched()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2026));
        var series = Series(1, "One Piece") with { SeriesSeason = season };
        var index = LibraryTitleIndex.Build([series]);

        var matched = index.TryMatch("One Piece");

        Assert.NotNull(matched);
        Assert.Equal(season, matched.Season);
    }

    [Fact]
    public void Should_Fuzzy_Match_When_Release_Title_Has_An_Extra_Word()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Kaiju No. 8")]);

        var matched = index.TryMatch("Kaiju No. 8 Movie");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
    }

    [Fact]
    public void Should_Fuzzy_Match_When_Release_Title_Is_Missing_A_Word()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Chuunibyou demo Koi ga Shitai")]);

        var matched = index.TryMatch("Chuunibyou Koi ga Shitai");

        Assert.NotNull(matched);
        Assert.Equal(1, matched.MalId);
    }

    [Fact]
    public void Should_Not_Fuzzy_Match_Genuinely_Different_Titles_Below_Threshold()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        Assert.Null(index.TryMatch("One Punch Man"));
    }

    private static TvSeries Series(int malId, params string[] allTitles) =>
        new(malId) { AllTitles = allTitles };
}
