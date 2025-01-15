using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.Features.Seasons.IO;
using AnimeFeedManager.Features.Seasons.Types;
using SeasonsGetter = AnimeFeedManager.Features.Seasons.SeasonsGetter;

namespace AnimeFeedManager.Features.Tests.Seasons;

public class GetAvailableSeasonsTests
{
    [Fact]
    public async Task Should_Order_Stored_Seasons_Correctly()
    {
        var mock = Substitute.For<ISeasonsGetter>();
        mock.GetAvailableSeasons(Arg.Any<CancellationToken>()).Returns(Right<DomainError, ImmutableList<SeasonStorage>>(TestData()));

        var latestSeasonMock = Substitute.For<ILatestSeasonsGetter>();
        
        var sut = new SeasonsGetter(mock,latestSeasonMock);
        var result = await sut.GetAvailable();

        result.Match(
            items => Assert.Equal(OrderedResults(), items),
            _ => Assert.Fail("Should not be here")
        );
    }


    private static ImmutableList<SeasonStorage> TestData()
    {
        var list = new List<SeasonStorage>
        {
            new() { Season = Season.Summer, Year = 2022, Latest = false },
            new() { Season = Season.Fall, Year = 2022, Latest = false },
            new() { Season = Season.Winter, Year = 2023, Latest = false },
            new() { Season = Season.Winter, Year = 2024, Latest = false },
            new() { Season = Season.Summer, Year = 2023, Latest = false },
            new() { Season = Season.Spring, Year = 2023, Latest = false },
            new() { Season = Season.Fall, Year = 2023, Latest = true }
        };

        return list.ToImmutableList();
    }

    private static ImmutableList<SimpleSeasonInfo> OrderedResults()
    {
        var list = new List<SimpleSeasonInfo>
        {
            new(Season.Winter, 2024, false),
            new(Season.Fall, 2023, true),
            new(Season.Summer, 2023, false),
            new(Season.Spring, 2023, false),
            new(Season.Winter, 2023, false),
            new(Season.Fall, 2022, false),
            new(Season.Summer, 2022, false)
        };

        return list.ToImmutableList();
    }
}