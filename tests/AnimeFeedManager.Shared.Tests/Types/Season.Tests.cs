namespace AnimeFeedManager.Shared.Tests.Types;

public class SeasonTests
{
    #region CompareTo Tests

    [Theory]
    [InlineData("winter")]
    [InlineData("spring")]
    [InlineData("summer")]
    [InlineData("fall")]
    public void Should_Return_Zero_When_Seasons_Are_Equal(string value)
    {
        var season = Season.FromString(value);

        Assert.Equal(0, season.CompareTo(Season.FromString(value)));
    }

    [Theory]
    [InlineData("winter", "spring")]
    [InlineData("spring", "summer")]
    [InlineData("summer", "fall")]
    [InlineData("winter", "fall")]
    public void Should_Return_Negative_When_Season_Precedes_Other(string earlier, string later)
    {
        var season = Season.FromString(earlier);

        Assert.True(season.CompareTo(Season.FromString(later)) < 0);
    }

    [Theory]
    [InlineData("spring", "winter")]
    [InlineData("summer", "spring")]
    [InlineData("fall", "summer")]
    [InlineData("fall", "winter")]
    public void Should_Return_Positive_When_Season_Follows_Other(string later, string earlier)
    {
        var season = Season.FromString(later);

        Assert.True(season.CompareTo(Season.FromString(earlier)) > 0);
    }

    #endregion
}
