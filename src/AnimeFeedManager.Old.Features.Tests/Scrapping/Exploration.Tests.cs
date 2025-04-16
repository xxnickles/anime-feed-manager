using System.Net;
using System.Text.RegularExpressions;

namespace AnimeFeedManager.Old.Features.Tests.Scrapping;

public partial class ExplorationTests
{
    // https://nyaa.si/?f=2&c=1_0&q=Koukaku+Kidoutai%3A+SAC_2045+%282022%29
    
    [Theory]
    [InlineData("Digimon Adventure 02: The Beginning", "Digimon+Adventure+02%3A+The+Beginning")]
    [InlineData("Koukaku Kidoutai: SAC_2045 (2022)", "Koukaku+Kidoutai%3A+SAC_2045+(2022)")]
    public void Should_Enconde_String_WebUtility_Method(string source, string expected)
    {
        var sut = WebUtility.UrlEncode(source);
        Assert.Equal(expected, sut);
    }

    [Theory]
    [InlineData("[Erai-raws] Saikyou Onmyouji no Isekai Tenseiki - 12 [480p][Multiple Subtitle] [ENG][POR-BR][SPA-LA][SPA][FRE][GER][ITA][RUS]", "[Erai-raws] Saikyou Onmyouji no Isekai Tenseiki - 12 [480p][Multiple Subtitle]")]
    [InlineData("[Erai-raws] Monsters - Ippyaku Sanjou Hiryuu Jigoku - ONA [1080p][HEVC][Multiple Subtitle] [ENG][POR-BR][SPA-LA][SPA][ARA][FRE][GER][ITA][JPN][POR][POL][TUR][IND][THA][KOR][CHI][MAY]", "[Erai-raws] Monsters - Ippyaku Sanjou Hiryuu Jigoku - ONA [1080p][HEVC][Multiple Subtitle]")]
    [InlineData("[Erai-raws] Boku no Daemon - 01 ~ 13 [720p][BATCH][Multiple Subtitle] [ENG][POR-BR][SPA-LA][SPA][ARA][FRE][GER][ITA][RUS][JPN][POR][POL][DUT][NOB][FIN][TUR][SWE][GRE][HEB][RUM][IND][THA][KOR][DAN][CHI][VIE][UKR][HUN][CES][HRV][MAY][FIL]","[Erai-raws] Boku no Daemon - 01 ~ 13 [720p][BATCH][Multiple Subtitle]")]
    public void Should_Remove_Language_Strings(string title, string expected)
    {
        var sut = TitlePattern().Replace(title, "").Trim();
        Assert.Equal(expected, sut);
    }

    [GeneratedRegex(@"\[[A-Z]{3}(-[A-Z]+)?\]")]
    private static partial Regex TitlePattern();
}