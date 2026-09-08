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
}
