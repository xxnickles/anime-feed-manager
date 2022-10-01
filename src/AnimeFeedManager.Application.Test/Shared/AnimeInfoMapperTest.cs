using AnimeFeedManager.Application.Mappers;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Application.Test.Shared;

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
            new SeasonInformation(Season.Spring, Year.FromNumber(2015)),
            None,
            false);


        var sut = AnimeInfoMappers.ProjectToStorageModel(animeInfo);
        Assert.Null(sut.Date);
    }
}