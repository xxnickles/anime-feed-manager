using System.Collections.Immutable;
using AnimeFeedManager.Services.Collectors.AniDb;
using AnimeFeedManager.Services.Collectors.SubsPlease;
using AnimeFeedManager.Storage.Infrastructure;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class FeedProviderTest : WithScrapper
{
    [Fact(Skip = "Takes too long in Git Actions")]
    public void SubsPlease_Feed_Works()
    {
        var sut = new FeedProvider(BrowserOptions).GetFeed(Resolution.Hd);
        Assert.True(sut.IsRight);
        sut.Match(
            val =>
            {
                Assert.DoesNotContain(val, x => x.PublicationDate < DateTime.Today);
                foreach (var feed in val)
                {
                    Assert.True(feed.AnimeTitle.Value.IsSome, $"{feed.FeedTitle} has no title");
                    Assert.True(feed.Links.Any());
                    Assert.True(!string.IsNullOrWhiteSpace(feed.EpisodeInfo), $"{feed.FeedTitle} has no episode info");
                }
            },
            _ => { });
    }

    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task SubsPlease_Feed_Titles_Works()
    {
        var sut = await new FeedProvider(BrowserOptions).GetTitles();
        Assert.True(sut.IsRight);
        sut.Match(
            Assert.NotEmpty,
            _ => Assert.True(false, "An error happened getting titles"));
    }


    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task Library_Works()
    {
        var mock = new Mock<IDomainPostman>();
        var sut = await new TvSeriesProvider(mock.Object, BrowserOptions).GetLibrary(new[] {"a", "b", "c"}.ToImmutableList());
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