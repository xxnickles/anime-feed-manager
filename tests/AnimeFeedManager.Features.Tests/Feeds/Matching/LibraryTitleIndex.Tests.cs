using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Matching;

public class LibraryTitleIndexTests
{
    [Fact]
    public void Should_Match_By_Default_Title()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        Assert.True(index.TryMatch("One Piece", out var seriesId));
        Assert.Equal(1, seriesId);
    }

    [Fact]
    public void Should_Match_By_Any_Title_Variant()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Grow Up Show: Himawari no Circus-dan", "Grow Up Show - Himawari no Circus-dan")]);

        Assert.True(index.TryMatch("Grow Up Show - Himawari no Circus-dan", out var seriesId));
        Assert.Equal(1, seriesId);
    }

    [Fact]
    public void Should_Match_Regardless_Of_Casing_And_Punctuation_Differences()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Mahou Shoujo Lyrical Nanoha Exceeds: Gun Blaze Vengeance")]);

        Assert.True(index.TryMatch("mahou shoujo lyrical nanoha EXCEEDS - Gun Blaze Vengeance", out var seriesId));
        Assert.Equal(1, seriesId);
    }

    [Fact]
    public void Should_Return_False_When_No_Series_Matches()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        Assert.False(index.TryMatch("Naruto", out _));
    }

    [Fact]
    public void Should_Skip_Titles_That_Normalize_To_Nothing_Meaningful()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece", "ワンピース")]); // Japanese title, non-ASCII

        Assert.True(index.TryMatch("One Piece", out var seriesId));
        Assert.Equal(1, seriesId);
        Assert.False(index.TryMatch("", out _));
    }

    [Fact]
    public void Should_Build_Empty_Index_When_Library_Is_Empty()
    {
        var index = LibraryTitleIndex.Build([]);

        Assert.False(index.TryMatch("Anything", out _));
    }

    [Fact]
    public void Should_Fuzzy_Match_When_Release_Title_Has_An_Extra_Word()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Kaiju No. 8")]);

        Assert.True(index.TryMatch("Kaiju No. 8 Movie", out var seriesId));
        Assert.Equal(1, seriesId);
    }

    [Fact]
    public void Should_Fuzzy_Match_When_Release_Title_Is_Missing_A_Word()
    {
        var index = LibraryTitleIndex.Build([Series(1, "Chuunibyou demo Koi ga Shitai")]);

        Assert.True(index.TryMatch("Chuunibyou Koi ga Shitai", out var seriesId));
        Assert.Equal(1, seriesId);
    }

    [Fact]
    public void Should_Not_Fuzzy_Match_Genuinely_Different_Titles_Below_Threshold()
    {
        var index = LibraryTitleIndex.Build([Series(1, "One Piece")]);

        Assert.False(index.TryMatch("One Punch Man", out _));
    }

    private static TvSeries Series(int malId, params string[] allTitles) =>
        new(malId) { AllTitles = allTitles };
}
