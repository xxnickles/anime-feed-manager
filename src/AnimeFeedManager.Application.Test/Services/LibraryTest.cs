using System.Collections.Immutable;
using AnimeFeedManager.Services.Collectors.AniDb;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class LibraryTes
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

}