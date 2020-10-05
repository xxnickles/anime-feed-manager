using System.Collections.Immutable;
using System.Security;
using AnimeFeedManager.Services.Collectors.LiveChart;
using AnimeFeedManager.Storage.Interface;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AnimeFeedManager.Application.Test.Services
{
    [Trait("Category", "Services")]
    public class LiveChartLibraryTest
    {

        [Fact]
        public async Task LibraryWorks()
        {
            var mock = new Mock<IFeedTitlesRepository>();

            mock.Setup(x => x.GetTitles()).ReturnsAsync((new[] {"a", "b", "c"}).ToImmutableList());
            var sut = await new LibraryProvider(mock.Object).GetLibrary();
            Assert.True(sut.IsRight);
            sut.Match(
                Assert.NotEmpty,
                _ => { }
            );

        }
    }
}
