using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Test.Extensions;

[Trait("Category", "Extensions")]
public class SeasonInformationTest
{
    [Fact]
    public void ShouldOrderSeasonInformation()
    {
        var seasons = new[]
        {
            new SeasonInformation(Season.Fall, new Year(2019)),
            new SeasonInformation(Season.Spring, new Year(2019)),
            new SeasonInformation(Season.Spring, new Year(2018)),
            new SeasonInformation(Season.Summer, new Year(2019)),
            new SeasonInformation(Season.Winter, new Year(2019)),
            new SeasonInformation(Season.Winter, new Year(2020))
        };

        var sut = seasons.Order();

        Assert.Equal(sut[0].Season, Season.Winter);
        Assert.Equal(sut[0].Year.Value, Some((ushort)2020));

        Assert.Equal(sut[1].Season, Season.Fall);
        Assert.Equal(sut[1].Year.Value, Some((ushort)2019));

        Assert.Equal(sut[2].Season, Season.Summer);
        Assert.Equal(sut[2].Year.Value, Some((ushort)2019));

        Assert.Equal(sut[3].Season, Season.Spring);
        Assert.Equal(sut[3].Year.Value, Some((ushort)2019));

        Assert.Equal(sut[4].Season, Season.Winter);
        Assert.Equal(sut[4].Year.Value, Some((ushort)2019));

        Assert.Equal(sut[5].Season, Season.Spring);
        Assert.Equal(sut[5].Year.Value, Some((ushort)2018));
    }
}