using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.Search;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Search;

public class ReleaseSearchTests
{
    private static readonly DateTimeOffset Published = new(2026, 11, 1, 16, 0, 0, TimeSpan.Zero);

    #region Candidates Tests

    [Fact]
    public void Should_Offer_Only_1080p_Variants_When_Search_Mixes_Resolutions()
    {
        ImmutableArray<NyaaItem> items =
        [
            Item("[Doomdos] - Kanojo no Tomodachi - S01E05 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "a"),
            Item("[ToonsHub] My Girlfriends Friend S01E05 720p UNCENSORED OV WEB-DL AAC2.0 H.264 (Kanojo no Tomodachi, English-Sub)", "b"),
            Item("[ToonsHub] My Girlfriends Friend S01E04 1080p UNCENSORED ADN WEB-DL AAC2.0 H.264 (Kanojo no Tomodachi, Multi-Subs)", "c")
        ];

        var candidates = ReleaseSearch.Candidates(items, new NoBinding());

        var candidate = Assert.Single(candidates);
        Assert.Equal("ToonsHub", candidate.Group);
        Assert.Contains("1080p", candidate.Variant);
    }

    [Fact]
    public void Should_Offer_No_Candidates_When_Every_Release_Is_Below_1080p()
    {
        ImmutableArray<NyaaItem> items =
        [
            Item("[Doomdos] - Kanojo no Tomodachi - S01E05 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "a"),
            Item("[Doomdos] - Kanojo no Tomodachi - S01E04 [Uncensored] [720p OV WEB-DL] [My Girlfriend's Friend]", "b")
        ];

        Assert.Empty(ReleaseSearch.Candidates(items, new NoBinding()));
    }

    [Fact]
    public void Should_Flag_Remake_When_Any_Item_Of_The_Variant_Is_A_Remake()
    {
        ImmutableArray<NyaaItem> items =
        [
            Item("[ASW] Hotel Inhumans S2 - 05 [1080p HEVC x265 10Bit][AAC]", "a", remake: true),
            Item("[ASW] Hotel Inhumans S2 - 04 [1080p HEVC x265 10Bit][AAC]", "b")
        ];

        var candidate = Assert.Single(ReleaseSearch.Candidates(items, new NoBinding()));

        Assert.True(candidate.Remake);
    }

    [Fact]
    public void Should_Not_Flag_Remake_When_No_Item_Is_A_Remake()
    {
        ImmutableArray<NyaaItem> items =
        [
            Item("[SubsPlease] Hotel Inhumans S2 - 06 (1080p) [7E30AE87].mkv", "a", trusted: true),
            Item("[SubsPlease] Hotel Inhumans S2 - 05 (1080p) [F22204B4].mkv", "b", trusted: true)
        ];

        var candidate = Assert.Single(ReleaseSearch.Candidates(items, new NoBinding()));

        Assert.False(candidate.Remake);
    }

    [Fact]
    public void Should_Not_Mark_Trusted_When_Any_Item_Of_The_Variant_Is_Untrusted()
    {
        ImmutableArray<NyaaItem> items =
        [
            Item("[SubsPlease] Hotel Inhumans S2 - 06 (1080p) [7E30AE87].mkv", "a", trusted: true),
            Item("[SubsPlease] Hotel Inhumans S2 - 05 (1080p) [F22204B4].mkv", "b")
        ];

        var candidate = Assert.Single(ReleaseSearch.Candidates(items, new NoBinding()));

        Assert.False(candidate.Trusted);
    }

    #endregion

    #region Test Helpers

    private static NyaaItem Item(string title, string infoHash, bool trusted = false, bool remake = false) =>
        new(title, "magnet:?", "https://nyaa.si/download/1.torrent", infoHash,
            Published, "700 MiB", Trusted: trusted, Remake: remake, Category: "1_2");

    #endregion
}
