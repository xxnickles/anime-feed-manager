using System;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    public class FeedProviderTest
    {
        [Fact]
        public void FeedWorks()
        {
            var sut = new FeedProvider().GetFeed(Resolution.Hd);
            Assert.True(sut.IsRight);
            sut.Match(
                val =>
                {
                    Assert.DoesNotContain(val, x => x.PublicationDate < DateTime.Today);
                },
                _ => { });

        }
    }
}
