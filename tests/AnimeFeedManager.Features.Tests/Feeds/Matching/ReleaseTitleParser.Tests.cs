using AnimeFeedManager.Features.Feeds.Matching;

namespace AnimeFeedManager.Features.Tests.Feeds.Matching;

public class ReleaseTitleParserTests
{
    #region Episode — Erai-raws shape (bracketed tech info, no parens)

    [Fact]
    public void Should_Parse_Episode_When_Erai_Raws_Format()
    {
        var result = ReleaseTitleParser.Parse(
            "[Erai-raws] Meitantei Precure - 24 [1080p CR WEB-DL AVC AAC][068DA821]");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Meitantei Precure", result.CleanTitle);
        Assert.Equal(24, episode.Number);
        Assert.False(result.IsBdRemux);
    }

    [Fact]
    public void Should_Parse_Episode_When_Title_Contains_Colon()
    {
        var result = ReleaseTitleParser.Parse(
            "[Erai-raws] Grow Up Show: Himawari no Circus-dan - 02 [1080p CR WEBRip HEVC AAC][MultiSub][C7373699]");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Grow Up Show: Himawari no Circus-dan", result.CleanTitle);
        Assert.Equal(2, episode.Number);
    }

    [Fact]
    public void Should_Parse_Long_Episode_Numbers()
    {
        var result = ReleaseTitleParser.Parse(
            "[Erai-raws] Detective Conan - 1206 [1080p CR WEB-DL AVC AAC][MultiSub][46D4E35B]");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Detective Conan", result.CleanTitle);
        Assert.Equal(1206, episode.Number);
    }

    #endregion

    #region Episode — SubsPlease shape (parenthesized resolution + hash + .mkv)

    [Fact]
    public void Should_Parse_Episode_When_SubsPlease_Format()
    {
        var result = ReleaseTitleParser.Parse("[SubsPlease] Meitantei Precure! - 24 (1080p) [A084C875].mkv");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Meitantei Precure!", result.CleanTitle);
        Assert.Equal(24, episode.Number);
    }

    [Fact]
    public void Should_Strip_Revision_Suffix_From_Episode_Number()
    {
        var result = ReleaseTitleParser.Parse("[SubsPlease] Yomi no Tsugai - 14v2 (1080p) [983D7BB5].mkv");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal(14, episode.Number);
    }

    #endregion

    #region Episode — tricky title shapes

    [Fact]
    public void Should_Not_Confuse_Hyphenated_Title_Word_With_Episode_Separator()
    {
        var result = ReleaseTitleParser.Parse(
            "[Erai-raws] Akane-banashi - 10 (REPACK) [1080p NF WEBRip HEVC AAC][MultiSub][6347BCFD]");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Akane-banashi", result.CleanTitle);
        Assert.Equal(10, episode.Number);
    }

    [Fact]
    public void Should_Find_The_Real_Episode_Separator_When_Title_Itself_Contains_Dash_Separated_Words()
    {
        var result = ReleaseTitleParser.Parse(
            "[SubsPlease] Mahou Shoujo Lyrical Nanoha EXCEEDS - Gun Blaze Vengeance - 02 (1080p) [46CF4D5F].mkv");

        var episode = Assert.IsType<ReleaseContent.SingleEpisode>(result.Content);
        Assert.Equal("Mahou Shoujo Lyrical Nanoha EXCEEDS - Gun Blaze Vengeance", result.CleanTitle);
        Assert.Equal(2, episode.Number);
    }

    #endregion

    #region Batch

    [Fact]
    public void Should_Parse_Batch_When_Parenthesized_Range_Present()
    {
        var result = ReleaseTitleParser.Parse("[SubsPlease] Otaku ni Yasashii Gal wa Inai (01-12) (1080p) [Batch]");

        var batch = Assert.IsType<ReleaseContent.Batch>(result.Content);
        Assert.Equal("Otaku ni Yasashii Gal wa Inai", result.CleanTitle);
        Assert.Equal(1, batch.Start);
        Assert.Equal(12, batch.End);
    }

    #endregion

    #region Non-numbered (movie/OVA)

    [Fact]
    public void Should_Parse_NonNumbered_When_No_Episode_Or_Batch_Marker_Present()
    {
        var result = ReleaseTitleParser.Parse("[SubsPlease] Some Movie Title (1080p) [ABCDEF12].mkv");

        Assert.IsType<ReleaseContent.NonNumbered>(result.Content);
        Assert.Equal("Some Movie Title", result.CleanTitle);
    }

    [Fact]
    public void Should_Parse_NonNumbered_When_Title_Has_No_Trailing_Brackets_At_All()
    {
        var result = ReleaseTitleParser.Parse("[SubsPlease] Some Movie Title");

        Assert.IsType<ReleaseContent.NonNumbered>(result.Content);
        Assert.Equal("Some Movie Title", result.CleanTitle);
    }

    #endregion

    #region BD/remux

    [Theory]
    [InlineData("[Judas] Sword Art Online (BD 1080p) [Batch]")]
    [InlineData("[Yameii] Series Name - 05 (BDRip 1080p HEVC)")]
    [InlineData("[Group] Series Name - 05 (Blu-Ray 1080p)")]
    [InlineData("[Group] Series Name - 05 [REMUX]")]
    public void Should_Set_IsBdRemux_When_Bd_Marker_Present(string rawTitle)
    {
        var result = ReleaseTitleParser.Parse(rawTitle);

        Assert.True(result.IsBdRemux);
    }

    [Fact]
    public void Should_Not_Set_IsBdRemux_When_No_Bd_Marker_Present()
    {
        var result = ReleaseTitleParser.Parse("[Erai-raws] Meitantei Precure - 24 [1080p CR WEB-DL AVC AAC][068DA821]");

        Assert.False(result.IsBdRemux);
    }

    [Fact]
    public void Should_Preserve_Batch_Range_When_Release_Is_Also_Bd_Remux()
    {
        var result = ReleaseTitleParser.Parse("[Judas] Sword Art Online (01-25) (BD 1080p) [Batch]");

        var batch = Assert.IsType<ReleaseContent.Batch>(result.Content);
        Assert.Equal(1, batch.Start);
        Assert.Equal(25, batch.End);
        Assert.True(result.IsBdRemux);
    }

    #endregion
}
