namespace AnimeFeedManager.Shared.Tests.Types;

public class SeriesSeasonExtensionsTests
{
    #region ToDisplayLabel

    [Theory]
    [InlineData("spring", 2026, "Spring 2026")]
    [InlineData("winter", 2025, "Winter 2025")]
    [InlineData("summer", 2024, "Summer 2024")]
    [InlineData("fall", 2023, "Fall 2023")]
    public void Should_Title_Case_Season_And_Append_Year(string season, int year, string expected)
    {
        var seriesSeason = new SeriesSeason(Season.FromString(season), Year.FromNumber(year));

        Assert.Equal(expected, seriesSeason.ToDisplayLabel());
    }

    #endregion
}
