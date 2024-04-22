using AnimeFeedManager.Common;
using AnimeFeedManager.Features.Nyaa;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Tests.Scrapping;

public class NyaaScrapperTests
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public NyaaScrapperTests()
    {
        var fetcherOptions = new BrowserFetcherOptions();


        var browserFetcher = new BrowserFetcher(fetcherOptions);
        browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId).GetAwaiter().GetResult();
        var executablePath = browserFetcher.GetInstalledBrowsers().Last(b => b.Browser is SupportedBrowser.Chrome)
            .GetExecutablePath();

        _puppeteerOptions = new PuppeteerOptions(executablePath, false);
    }

    [Fact(Skip = "Should not be executed in pipelines as requires puppeter")]
    // [Fact]
    public async Task Should_Get_Series_List()
    {
        var sut = await NyaaScrapper.Scrap("Tonikaku Kawaii: Joshikou Hen", _puppeteerOptions);
        sut.Should().NotBeEmpty();
    }

    [Fact(Skip = "Should not be executed in pipelines as requires puppeter")]
    // [Fact]
    public async Task Should_Get_Empty_Series_List()
    {
        var sut = await NyaaScrapper.Scrap("nope", _puppeteerOptions);
        sut.Should().BeEmpty();
    }
}