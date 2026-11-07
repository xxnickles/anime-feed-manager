using AnimeFeedManager.Features.Scrapping.Nyaa;

namespace AnimeFeedManager.Features.Tests.Scrapping.Nyaa;

public class NyaaReleaseTitleTests
{
    #region Regular Release Tests

    [Fact]
    public void Should_Parse_Group_Title_Episode_And_Variant_When_Title_Is_SubsPlease_Shape()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Kusuriya no Hitorigoto - 53 (1080p) [6B17692F].mkv");

        result.AssertOnSuccess(release =>
        {
            Assert.Equal("SubsPlease", release.Group);
            Assert.Equal("Kusuriya no Hitorigoto", release.Title);
            Assert.Equal(new RegularEpisode(53, "53"), release.Episode);
            Assert.Equal(1, release.Version);
            Assert.False(release.Revised);
            Assert.Equal("(1080p)", release.Variant);
        });
    }

    [Fact]
    public void Should_Keep_Bracketed_Tags_Without_The_Crc_When_Building_The_Variant()
    {
        var result = NyaaReleaseTitle.Parse("[Erai-raws] Psyren - 03 [1080p CR WEBRip HEVC AAC][MultiSub][A4F9929A]");

        result.AssertOnSuccess(release => Assert.Equal("[1080p CR WEBRip HEVC AAC][MultiSub]", release.Variant));
    }

    [Fact]
    public void Should_Keep_Season_Words_In_The_Title_When_Episode_Follows_The_Separator()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show Season 2 - 03 (1080p) [ABCD1234].mkv");

        result.AssertOnSuccess(release =>
        {
            Assert.Equal("Show Season 2", release.Title);
            Assert.Equal(new RegularEpisode(3, "03"), release.Episode);
        });
    }

    [Fact]
    public void Should_Keep_Ordinal_Season_In_The_Title_When_Title_Says_2nd_Season()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show 2nd Season - 03 (1080p) [ABCD1234].mkv");

        result.AssertOnSuccess(release => Assert.Equal("Show 2nd Season", release.Title));
    }

    #endregion

    #region SxxEyy Tests

    [Fact]
    public void Should_Parse_SxxEyy_Episode_When_Title_Uses_Season_Episode_Form()
    {
        var result = NyaaReleaseTitle.Parse(
            "[Anime Time] Bleach: Thousand-Year Blood War - S01E49 [1080p][HEVC 10bit x265][AAC][Multi Sub] [Weekly] Bleach Sennen Kessen hen");

        result.AssertOnSuccess(release =>
        {
            Assert.Equal("Anime Time", release.Group);
            Assert.Equal("Bleach: Thousand-Year Blood War", release.Title);
            Assert.Equal(new RegularEpisode(49, "S01E49"), release.Episode);
        });
    }

    [Fact]
    public void Should_Parse_Title_When_A_Dash_Follows_The_Group()
    {
        var result = NyaaReleaseTitle.Parse("[Doomdos] - Thermae Romae - S01E05 [Dub] [1080p OV WEB-DL] [Thermae Romae]");

        result.AssertOnSuccess(release =>
        {
            Assert.Equal("Doomdos", release.Group);
            Assert.Equal("Thermae Romae", release.Title);
            Assert.Equal(5, Assert.IsType<RegularEpisode>(release.Episode).Number);
        });
    }

    [Fact]
    public void Should_Upper_Case_The_Label_When_SxxEyy_Is_Lower_Case()
    {
        var result = NyaaReleaseTitle.Parse("[Group] Show s01e05 [1080p]");

        result.AssertOnSuccess(release => Assert.Equal("S01E05", release.Episode.Label));
    }

    #endregion

    #region Version Tests

    [Fact]
    public void Should_Read_Version_When_Episode_Has_A_Version_Suffix()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Kusuriya no Hitorigoto - 49v2 (1080p) [F14E89B9].mkv");

        result.AssertOnSuccess(release =>
        {
            Assert.Equal(new RegularEpisode(49, "49"), release.Episode);
            Assert.Equal(2, release.Version);
            Assert.True(release.Revised);
            Assert.Equal("(1080p)", release.Variant);
        });
    }

    [Fact]
    public void Should_Count_A_Bare_Repack_As_Version_Two()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show - 05 (1080p) [REPACK] [ABCD1234].mkv");

        result.AssertOnSuccess(release => Assert.Equal(2, release.Version));
    }

    [Fact]
    public void Should_Keep_The_Higher_Version_When_A_Repack_Also_Has_A_Version()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show - 05v3 (1080p) [REPACK] [ABCD1234].mkv");

        result.AssertOnSuccess(release => Assert.Equal(3, release.Version));
    }

    [Fact]
    public void Should_Match_The_Original_Variant_When_Release_Is_A_Repack()
    {
        var original = NyaaReleaseTitle.Parse("[SubsPlease] Show - 05 (1080p) [ABCD1234].mkv");
        var repack = NyaaReleaseTitle.Parse("[SubsPlease] Show - 05 (1080p) [REPACK] [EF567890].mkv");

        original.AssertOnSuccess(first => repack.AssertOnSuccess(second => Assert.Equal(first.Variant, second.Variant)));
    }

    #endregion

    #region Special Episode Tests

    [Fact]
    public void Should_Parse_Episode_Zero_As_Special()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show - 00 (1080p) [ABCD1234].mkv");

        result.AssertOnSuccess(release => Assert.Equal(new SpecialEpisode(0, "00"), release.Episode));
    }

    [Fact]
    public void Should_Parse_Decimal_Episode_As_Special()
    {
        var result = NyaaReleaseTitle.Parse("[SubsPlease] Show - 12.5 (1080p) [ABCD1234].mkv");

        result.AssertOnSuccess(release => Assert.Equal(new SpecialEpisode(12.5m, "12.5"), release.Episode));
    }

    #endregion

    #region Rejected Title Tests

    [Theory]
    [InlineData("[SubsPlease] Nige Jouzu no Wakagimi S2 (01-12) (1080p) [Batch]")]
    [InlineData("[SubsPlease] Seihantai na Kimi to Boku (13-25) (1080p) [Batch]")]
    [InlineData("[Group] Show - 01-12 (1080p)")]
    [InlineData("[Group] Show - 01 ~ 12 [1080p]")]
    public void Should_Warn_When_Title_Is_A_Batch(string title)
    {
        NyaaReleaseTitle.Parse(title).AssertOnError(error => Assert.IsType<Warning>(error));
    }

    [Theory]
    [InlineData("MAGICAL GIRL RAISING PROJECT restart S01E02 1080p AMZN WEB-DL DDP2.0 H.264-VARYG (Mahou Shoujo Ikusei Keikaku: restart, Multi-Subs)")]
    [InlineData("[SubsPlease] Show (1080p) [ABCD1234].mkv")]
    [InlineData("")]
    public void Should_Warn_When_Title_Has_No_Group_Or_Episode(string title)
    {
        NyaaReleaseTitle.Parse(title).AssertOnError(error => Assert.IsType<Warning>(error));
    }

    #endregion
}
