using AnimeFeedManager.Services.Collectors.HorribleSubs;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    [Trait("Category", "Services")]
    public class FeedTitlesTests
    {
        [Fact]
        public void FeedTitlesWorks()
        {
            var sut = new FeedTitles().GetTitles();
            Assert.True(sut.IsRight);
            sut.Match(
                Assert.NotEmpty,
                _ => { });
        }
    }
}
