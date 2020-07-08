using AnimeFeedManager.Application.Shared.Mappers;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using Xunit;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Test.Shared
{
    public class AnimeInfoMapperTest
    {
        [Fact]
        public void ShouldMapWhenDateIsNone()
        {
            var animeInfo = new AnimeInfo(
                NonEmptyString.FromString("a"),
                NonEmptyString.FromString("title"),
                NonEmptyString.FromString(null),
                NonEmptyString.FromString("test"),
                new SeasonInformation(Season.Spring, new Year(2015)),
                None,
                false);


            var sut = AnimeInfoMappers.ProjectToStorageModel(animeInfo);
            Assert.Null(sut.Date);
        }
    }
}