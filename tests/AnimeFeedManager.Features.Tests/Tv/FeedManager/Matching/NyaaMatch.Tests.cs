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

    #region Identity Tests

    [Theory]
    [InlineData("Erai-raws", "Show", "(1080p)")]
    [InlineData("SubsPlease", "Show S2", "(1080p)")]
    [InlineData("SubsPlease", "show", "(1080p)")]
    [InlineData("SubsPlease", "Show", "(720p)")]
    public void Should_Not_Match_When_Group_Title_Or_Variant_Differ(string group, string title, string variant)
    {
        var release = new NyaaRelease(group, title, new RegularEpisode(1, "01"), 1, variant);

        Assert.False(NyaaMatch.Matches(Binding(premiere: null), Item(Premiere), release));
    }

    [Fact]
    public void Should_Reject_An_Untrusted_Item_When_Binding_Is_Trusted()
    {
        var binding = Binding(premiere: null);
        binding.Trusted = true;

        Assert.False(NyaaMatch.Matches(binding, Item(Premiere) with {Trusted = false}, Release));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Accept_Any_Item_When_Binding_Is_Not_Trusted(bool trustedItem)
    {
        Assert.True(NyaaMatch.Matches(Binding(premiere: null), Item(Premiere) with {Trusted = trustedItem}, Release));
    }

    #endregion

    #region CatalogRows Tests

    [Fact]
    public void Should_Skip_Items_Whose_Title_Is_Not_A_Single_Release()
    {
        var batch = Item(Premiere) with {Title = "[SubsPlease] Show (01-12) (1080p) [Batch]"};

        Assert.Empty(NyaaMatch.CatalogRows(Binding(premiere: null), "2026-fall-show", [batch]));
    }

    [Fact]
    public void Should_Catalog_One_Row_Per_Upload_When_The_Same_Item_Repeats()
    {
        var item = Item(Premiere, infoHash: "same");

        var rows = NyaaMatch.CatalogRows(Binding(premiere: null), "2026-fall-show", [item, item]);

        Assert.Equal("same", Assert.Single(rows).InfoHash);
    }

    [Fact]
    public void Should_Key_Rows_By_Series_Id()
    {
        var rows = NyaaMatch.CatalogRows(Binding(premiere: null), "2026-fall-show", [Item(Premiere)]);

        Assert.Equal("2026-fall-show", Assert.Single(rows).PartitionKey);
    }

    [Fact]
    public void Should_Catalog_Only_The_Bound_Variant_When_Group_Separates_Title_With_A_Dash()
    {
        var binding = new NyaaBindingStorage
        {
            Group = "Doomdos",
            ReleaseTitle = "Kanojo no Tomodachi",
            Variant = "[Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]"
        };
        NyaaItem[] items =
        [
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E05 [720p OV WEB-DL] [My Girlfriend's Friend]", "a"),
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E05 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "b"),
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E04 [720p OV WEB-DL] [My Girlfriend's Friend]", "c"),
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E04 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "d"),
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E03 [720p OV WEB-DL] [My Girlfriend's Friend]", "e"),
            DoomdosItem("[Doomdos] - Kanojo no Tomodachi - S01E03 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "f")
        ];

        var rows = NyaaMatch.CatalogRows(binding, "2026-fall-kanojo", items);

        Assert.Equal(["b", "d", "f"], rows.Select(row => row.InfoHash));
        Assert.Equal(["S01E05", "S01E04", "S01E03"], rows.Select(row => row.EpisodeLabel));
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

    private static NyaaItem DoomdosItem(string title, string infoHash) =>
        new(title, "magnet:?", "https://nyaa.si/download/1.torrent", infoHash,
            Premiere, "700 MiB", Trusted: false, Remake: false, Category: "1_2");

    #endregion
}
