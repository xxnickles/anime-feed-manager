using System.Collections.Immutable;
using System.Runtime.InteropServices;
using AnimeFeedManager.Services.Collectors.AniDb;
using PuppeteerSharp;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class LibraryTest
{
    public LibraryTest()
    {
        var bfOptions = new BrowserFetcherOptions();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            bfOptions.Path = Path.GetTempPath();
        }
        using var bf = new BrowserFetcher(bfOptions);
        bf.DownloadAsync(BrowserFetcher.DefaultRevision).GetAwaiter().GetResult();
    }
    
    [Fact]
    public async Task Default_Library_Works()
    {
        var mock = new Mock<IFeedTitlesRepository>();

        mock.Setup(x => x.GetTitles()).ReturnsAsync((new[] {"a", "b", "c"}).ToImmutableList());
        var sut = await new ExternalLibraryProvider(mock.Object).GetLibrary();
        Assert.True(sut.IsRight);
        sut.Match(
            Assert.NotEmpty,
            _ => { }
        );
    }


    // [Fact]
    // public async Task Library_Works()
    // {
    //     var mock = new Mock<IFeedTitlesRepository>();
    //
    //     mock.Setup(x => x.GetTitles()).ReturnsAsync((new[] {"a", "b", "c"}).ToImmutableList());
    //     var sut = await new LibraryProvider(mock.Object).GetLibrary();
    //     Assert.True(sut.IsRight);
    //     sut.Match(
    //         r =>
    //         {
    //             Assert.NotEmpty(r.Series);
    //             Assert.NotEmpty(r.Titles);
    //         },
    //     _ => { }
    //         );
    // }
}