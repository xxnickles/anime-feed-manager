using System.Threading.Tasks;
using AnimeFeedManager.Services.Collectors;
using AnimeFeedManager.Services.Collectors.Erai;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    [Trait("Category", "Services")]
    public class FeedTitlesTests
    {
        // Horrible subs has been closed
        //[Fact]
        //public void FeedTitlesWorks()
        //{
        //    var sut = new FeedTitles().GetTitles();
        //    Assert.True(sut.IsRight);
        //    sut.Match(
        //        Assert.NotEmpty,
        //        _ => { });
        //}

        [Fact]
        public async Task FeedTitlesWorks()
        {
            var sut = await new FeedTitles(new BrowserProvider()).GetTitles();
            Assert.True(sut.IsRight);
            sut.Match(
                Assert.NotEmpty,
                _ => { });
        }
    }
}
