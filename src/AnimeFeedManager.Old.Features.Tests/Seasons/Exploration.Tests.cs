using System.Collections.Immutable;
using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Old.Features.Seasons.Types;

namespace AnimeFeedManager.Old.Features.Tests.Seasons;

public class ExplorationTests
{
    // This is just an exploration to determinate the best way to sort Seasons 
    [Fact]
    public void Should_Sort_Correctly()
    {
        var seasons = new List<SeasonStorage>
        {
            new()
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-spring", Season = Season.Spring, Latest = false,
                Year = 2023
            },
            new()
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-summer", Season = Season.Summer, Latest = false,
                Year = 2023
            },
            new()
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-winter", Season = Season.Winter, Latest = false,
                Year = 2023
            },
            new()
            {
                PartitionKey = SeasonType.Latest, RowKey = "2024-winter", Season = Season.Winter, Latest = true,
                Year = 2024
            },
            new()
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-fall", Season = Season.Fall, Latest = false,
                Year = 2023
            },
            new()
            {
                PartitionKey = SeasonType.Season, RowKey = "2025-fall", Season = Season.Fall, Latest = false,
                Year = 2025
            }
        }.ToImmutableList();


        var result = seasons
            .GroupBy(wrapper => wrapper.Year)
            .Select(group => new
            {
                Year = group.Key,
                Seasons = group.Select(i => new
                {
                    Season = Season.FromString(i.Season), i.PartitionKey, i.Latest, i.RowKey, i.Year, i.Timestamp,
                    i.ETag
                })
            })
            .OrderBy(grouped => grouped.Year)
            .SelectMany(grouped => grouped.Seasons.OrderBy(s => s.Season).Select(season =>
                new SeasonStorage
                {
                    Season = season.Season,
                    PartitionKey = season.PartitionKey,
                    Latest = season.Latest,
                    RowKey = season.RowKey,
                    Year = season.Year,
                    Timestamp = season.Timestamp,
                    ETag = season.ETag
                }))
            .Reverse()
            .ToArray();


        SeasonStorage[] expectation =
        [
            new SeasonStorage
            {
                PartitionKey = SeasonType.Season, RowKey = "2025-fall", Season = Season.Fall, Latest = false,
                Year = 2025
            },
            new SeasonStorage
            {
                PartitionKey = SeasonType.Latest, RowKey = "2024-winter", Season = Season.Winter, Latest = true,
                Year = 2024
            },
            new SeasonStorage
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-fall", Season = Season.Fall, Latest = false,
                Year = 2023
            },
            new SeasonStorage
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-summer", Season = Season.Summer, Latest = false,
                Year = 2023
            },
            new SeasonStorage
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-spring", Season = Season.Spring, Latest = false,
                Year = 2023
            },
            new SeasonStorage
            {
                PartitionKey = SeasonType.Season, RowKey = "2023-winter", Season = Season.Winter, Latest = false,
                Year = 2023
            }
        ];
        for (int i = 0; i < result.Length; i++)
        {
            Assert.Equal(expectation[i].Year, result[i].Year);
        }
    }
}