using System.Net;

namespace AnimeFeedManager.Features.Tests.Scrapping;

public class ExplorationTests
{
    // https://nyaa.si/?f=2&c=1_0&q=Koukaku+Kidoutai%3A+SAC_2045+%282022%29
    
    [Theory]
    [InlineData("Digimon Adventure 02: The Beginning", "Digimon+Adventure+02%3A+The+Beginning")]
    [InlineData("Koukaku Kidoutai: SAC_2045 (2022)", "Koukaku+Kidoutai%3A+SAC_2045+(2022)")]
    public void Should_Enconde_String_WebUtility_Method(string source, string expected)
    {
        var sut = WebUtility.UrlEncode(source);
        sut.Should().Be(expected);
    }
}