using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;
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
            _ => Assert.Fail("An error happened getting titles"));
    }


    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task Tv_Library_Works()
    {
        var mock = Substitute.For<IDomainPostman>();
        var sut = await new TvSeriesProvider(mock, BrowserOptions).GetLibrary(new[] {"a", "b", "c"}.ToImmutableList());
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.SeriesList);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
    
    [Fact]
    public async Task Ovas_Library_Works()
    {
        var mock = Substitute.For<IDomainPostman>();
        var sut = await new OvasProvider(mock, BrowserOptions).GetLibrary();
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.SeriesList);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
    
    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task Ovas_Library_By_Season_Works()
    {
        var mock = Substitute.For<IDomainPostman>();
        var sut = await new OvasProvider(mock, BrowserOptions).GetLibrary(new SeasonInformation(Season.Spring, Year.FromNumber(2022)));
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.SeriesList);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
    
    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task Movies_Library_Works()
    {
        var mock = Substitute.For<IDomainPostman>();
        var sut = await new MoviesProvider(mock, BrowserOptions).GetLibrary();
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.SeriesList);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
    
    [Fact(Skip = "Takes too long in Git Actions")]
    public async Task Movies_Library_By_Season_Works()
    {
        var mock = Substitute.For<IDomainPostman>();
        var sut = await new MoviesProvider(mock, BrowserOptions).GetLibrary(new SeasonInformation(Season.Spring, Year.FromNumber(2022)));
        Assert.True(sut.IsRight);
        sut.Match(
            r =>
            {
                Assert.NotEmpty(r.SeriesList);
                Assert.NotEmpty(r.Images);
            },
            _ => { }
        );
    }
}