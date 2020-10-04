using AnimeFeedManager.Services.Collectors.Erai;
using AnimeFeedManager.Services.Collectors.LiveChart;
using System.Threading.Tasks;
using AnimeFeedManager.Services.Collectors;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    [Trait("Category", "Services")]
    public class LiveChartLibraryTest
    {

        [Fact]
        public async Task LibraryWorks()
        {
            var feedTitlesService = new FeedTitles(new BrowserProvider());
            var sut = await new LibraryProvider(feedTitlesService).GetLibrary();
            Assert.True(sut.IsRight);
            sut.Match(
                Assert.NotEmpty,
                _ => { }
            );

        }
    }
}
