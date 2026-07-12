using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Matching;

public class NyaaReleaseMatcherTests
{
    [Fact]
    public void Should_Return_Matched_Release_When_Title_Matches_Library()
    {
        var index = LibraryTitleIndex.Build([new TvSeries(21) { AllTitles = ["Meitantei Precure"] }]);
        var entry = new NyaaEntry(
            "[Erai-raws] Meitantei Precure - 24 [1080p CR WEB-DL AVC AAC][068DA821]",
            "https://nyaa.si/download/1.torrent", "https://nyaa.si/view/1", DateTimeOffset.UtcNow);

        var result = NyaaReleaseMatcher.Match(entry, index);

        Assert.NotNull(result);
        Assert.Equal(21, result.SeriesId);
        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal(24, episode.Number);
        Assert.Same(entry, result.Entry);
    }

    [Fact]
    public void Should_Return_Null_When_No_Series_Matches()
    {
        var index = LibraryTitleIndex.Build([new TvSeries(21) { AllTitles = ["One Piece"] }]);
        var entry = new NyaaEntry(
            "[SubsPlease] Some Unrelated Show - 05 (1080p) [ABCDEF12].mkv",
            "https://nyaa.si/download/2.torrent", "https://nyaa.si/view/2", DateTimeOffset.UtcNow);

        var result = NyaaReleaseMatcher.Match(entry, index);

        Assert.Null(result);
    }
}
