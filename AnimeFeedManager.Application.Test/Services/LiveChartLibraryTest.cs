using AnimeFeedManager.Services.Collectors.HorribleSubs;
using AnimeFeedManager.Services.Collectors.LiveChart;
using System.Threading.Tasks;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    public class LiveChartLibraryTest
    {

        [Fact]
        public async Task LibraryWorks()
        {
            var feedTitlesService = new FeedTitles();
            var sut = await new LibraryProvider(feedTitlesService).GetLibrary();
            Assert.True(sut.IsRight);
            sut.Match(
                Assert.NotEmpty,
                _ => { }
            );

        }
    }
}
