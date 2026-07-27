using AnimeFeedManager.Features.Library.Events;

namespace AnimeFeedManager.Features.Tests.Library.Events;

public class SeasonImportedExtensionsTests
{
    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));

    #region ToSummary

    [Fact]
    public void Should_Sum_And_Join_ByType_Counts_In_Order()
    {
        var evt = new SeasonImported(Spring2026, null,
            [new SeriesTypeCount("tv", "TV", 8), new SeriesTypeCount("movie", "Movie", 3)],
            DateTimeOffset.UtcNow);

        Assert.Equal("11 series imported for Spring 2026 (8 TV, 3 Movie)", evt.ToSummary());
    }

    [Fact]
    public void Should_Render_A_Single_Type_Without_A_Separator()
    {
        var evt = new SeasonImported(Spring2026, null,
            [new SeriesTypeCount("tv", "TV", 5)], DateTimeOffset.UtcNow);

        Assert.Equal("5 series imported for Spring 2026 (5 TV)", evt.ToSummary());
    }

    #endregion
}
