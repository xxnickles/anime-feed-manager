using System;
using System.Linq;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Services.Collectors.SubsPlease;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services;

[Trait("Category", "Services")]
public class FeedProviderTest
{
    // Deprecated due to horriblesubs closing
    //[Fact]
    //public void FeedWorks()
    //{
    //    var sut = new FeedProvider().GetFeed(Resolution.Hd);
    //    Assert.True(sut.IsRight);
    //    sut.Match(
    //        val =>
    //        {
    //            Assert.DoesNotContain(val, x => x.PublicationDate < DateTime.Today);
    //        },
    //        _ => { });

    //}

    //[Fact]
    //public void EraiFeedWorks()
    //{
    //    var sut = new AnimeFeedManager.Services.Collectors.Erai.FeedProvider().GetFeed(Resolution.Hd);
    //    Assert.True(sut.IsRight);
    //    sut.Match(
    //        val =>
    //        {
    //            Assert.DoesNotContain(val, x => x.PublicationDate < DateTime.Today);
    //            foreach (var feed in val)
    //            {
    //                Assert.True(feed.AnimeTitle.Value.IsSome, $"{feed.FeedTitle} has no title");
    //                Assert.True(feed.Links.Any());
    //                Assert.True(!string.IsNullOrWhiteSpace(feed.EpisodeInfo), $"{feed.FeedTitle} has no episode info");
    //            }
    //        },
    //        _ => { });

    //}

    [Fact]
    public void SubsPleaseFeedWorks()
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
}