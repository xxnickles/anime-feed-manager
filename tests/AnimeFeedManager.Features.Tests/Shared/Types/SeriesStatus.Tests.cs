using System.Text.Json;
using AnimeFeedManager.Features.Common;
using CommonJsonContext = AnimeFeedManager.Features.Common.CommonJsonContext;

namespace AnimeFeedManager.Features.Tests.Shared.Types;

public class SeriesStatusTests
{
    [Fact]
    public void Should_Serialize_And_Deserialize_One_Instance()
    {
        var sut = ("fall", 2025, false).ParseAsSeriesSeason();
        sut.AssertOnSuccess(s =>
        {
            var serialized = JsonSerializer.Serialize(s, CommonJsonContext.Default.SeriesSeason);
            var deserialized = JsonSerializer.Deserialize(serialized, CommonJsonContext.Default.SeriesSeason);
            Assert.Equal(s, deserialized);
        });
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_Array()
    {
        SeriesSeason[] sut =
        [
            new(Season.Fall(), Year.FromNumber(2025)),
            new(Season.Winter(), Year.FromNumber(2025), true),
            new(Season.Spring(), Year.FromNumber(2025))
        ];

        var serialized = JsonSerializer.Serialize(sut, CommonJsonContext.Default.SeriesSeasonArray);
        var deserialized = JsonSerializer.Deserialize(serialized, CommonJsonContext.Default.SeriesSeasonArray);
        Assert.Equivalent(sut, deserialized);
    }

    [Theory]
    [InlineData("""{ "season" : "falls", "year" : 2025, "isLatest" : false}""")]
    [InlineData("""{ "season" : "fall", "year" : 1990, "isLatest" : false}""")]
    [InlineData("""{ "season" : , "year" : 30000}""")]
    public void Should_Not_Deserialize_Incorrect_Values(string seasonString)
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SeriesSeason>(seasonString, CommonJsonContext.Default.SeriesSeason)
        );
    }

    [Theory]
    [InlineData(
        """[{"season":"springg","year":2025,"isLatest":false},{"season":"autum","year":2023,"isLatest":true}]""")]
    [InlineData(
        """[{"season":"summer","year":1999,"isLatest":false},{"season":"winter","year":2025,"isLatest":true}]""")]
    [InlineData(
        """[{"season":"falll","year":2022,"isLatest":false},{"season":"winnter","year":2024,"isLatest":false}]""")]
    [InlineData(
        """[{"season":"spring","year":2127,"isLatest":true},{"season":"summer","year":2028,"isLatest":false}]""")]
    public void Should_Not_Deserialize_Incorrect_Array_Values(string seasonString)
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SeriesSeason[]>(seasonString, CommonJsonContext.Default.SeriesSeasonArray)
        );
    }
}