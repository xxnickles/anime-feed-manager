using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Matching;

public class NyaaMatchTests
{
    private static readonly DateTimeOffset Premiere = new(2026, 10, 3, 15, 0, 0, TimeSpan.Zero);
    private static readonly NyaaRelease Release = new("SubsPlease", "Show", new RegularEpisode(1, "01"), 1, "(1080p)");

    #region Premiere Tests

    [Fact]
    public void Should_Match_When_Binding_Has_No_Premiere()
    {
        var binding = Binding(premiere: null);

        var result = NyaaMatch.Matches(binding, Item(Premiere.AddYears(-1)), Release);

        Assert.True(result);
    }

    [Fact]
    public void Should_Match_When_Published_After_Premiere()
    {
        var result = NyaaMatch.Matches(Binding(Premiere), Item(Premiere.AddDays(7)), Release);

        Assert.True(result);
    }

    [Fact]
    public void Should_Match_When_Published_Exactly_At_The_Margin()
    {
        var result = NyaaMatch.Matches(Binding(Premiere), Item(Premiere - NyaaMatch.PremiereMargin), Release);

        Assert.True(result);
    }

    [Fact]
    public void Should_Match_When_Stream_Ahead_Release_Precedes_Premiere_Within_Margin()
    {
        var result = NyaaMatch.Matches(Binding(Premiere), Item(Premiere.AddDays(-6)), Release);

        Assert.True(result);
    }

    [Fact]
    public void Should_Not_Match_When_Published_Before_The_Margin()
    {
        var published = Premiere - NyaaMatch.PremiereMargin - TimeSpan.FromSeconds(1);

        var result = NyaaMatch.Matches(Binding(Premiere), Item(published), Release);

        Assert.False(result);
    }

    [Fact]
    public void Should_Skip_Previous_Cour_Rows_When_Cataloguing()
    {
        var items = new[]
        {
            Item(Premiere.AddDays(-90), infoHash: "old"),
            Item(Premiere.AddDays(1), infoHash: "new")
        };

        var rows = NyaaMatch.CatalogRows(Binding(Premiere), "2026-fall-show", items);

        Assert.Equal(["new"], rows.Select(row => row.InfoHash));
    }

    #endregion

    #region Test Helpers

    private static NyaaBindingStorage Binding(DateTimeOffset? premiere) =>
        new()
        {
            Group = Release.Group,
            ReleaseTitle = Release.Title,
            Variant = Release.Variant,
            Premiere = premiere
        };

    private static NyaaItem Item(DateTimeOffset published, string infoHash = "hash") =>
        new("[SubsPlease] Show - 01 (1080p)", "magnet:?", "https://nyaa.si/download/1.torrent", infoHash,
            published, "1.4 GiB", Trusted: true, Remake: false, Category: "1_2");

    #endregion
}
