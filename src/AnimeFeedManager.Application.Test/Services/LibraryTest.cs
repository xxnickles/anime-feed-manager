using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Services.Collectors.AniDb;
using LanguageExt;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class LibraryTest : WithScrapper
{
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

    [Fact]
    public async Task Library_Works()
    {
        var sut = await new LibraryProvider().GetLibrary(new[] {"a", "b", "c"}.ToImmutableList());
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.Series);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
}