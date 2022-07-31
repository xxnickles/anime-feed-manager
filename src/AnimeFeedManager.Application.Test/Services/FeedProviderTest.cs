using AnimeFeedManager.Services.Collectors.SubsPlease;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class FeedProviderTest : WithScrapper
{
    [Fact]
    public void SubsPlease_Feed_Works()
    {
        var sut = new FeedProvider().GetFeed(Resolution.Hd);
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
    
    [Fact]
    public async Task SubsPlease_Feed_Titles_Works()
    {
        var sut = await new FeedProvider().GetTitles();
        Assert.True(sut.IsRight);
        sut.Match(
            Assert.NotEmpty,
            _ => Assert.True(false, "An error happened getting titles"));
    }
}