using System.Text.Json;
using AnimeFeedManager.Features.Common;

namespace AnimeFeedManager.Features.Tests.Common;

public class SeriesSeasonTests
{
    #region ParseAsSeriesSeason (string)

    [Fact]
    public void Parses_A_Well_Formed_Season_String()
    {
        "summer-2026".ParseAsSeriesSeason().AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Summer(), season.Season);
            Assert.Equal(2026, season.Year);
            Assert.False(season.IsLatest);
        });
    }

    [Theory]
    [InlineData("summer-notayear")]
    [InlineData("summer-")]
    [InlineData("foo-bar")]
    [InlineData("summer-2026.5")]
    public void Returns_Failure_When_The_Year_Is_Not_A_Number(string value)
    {
        value.ParseAsSeriesSeason().AssertError();
    }

    [Theory]
    [InlineData("summer")]
    [InlineData("summer-2026-extra")]
    [InlineData("")]
    public void Returns_Failure_When_The_Shape_Is_Wrong(string value)
    {
        value.ParseAsSeriesSeason().AssertError();
    }

    [Fact]
    public void Returns_Failure_When_The_Season_Name_Is_Unknown()
    {
        "monsoon-2026".ParseAsSeriesSeason().AssertError();
    }

    #endregion

    #region JSON serialization

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

    #endregion
}
