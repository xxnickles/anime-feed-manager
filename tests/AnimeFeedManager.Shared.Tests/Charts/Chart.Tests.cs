using AnimeFeedManager.Shared.Charts;

namespace AnimeFeedManager.Shared.Tests.Charts;

public class ChartTests
{
    [Fact]
    public void Should_Take_Labels_From_Slices_When_Chart_Is_DonutChart()
    {
        var chart = new DonutChart(
        [
            new ChartSlice("Tracking", 5, ChartColor.Success),
            new ChartSlice("Stale", 1, ChartColor.Warning)
        ]);

        Assert.Equal(["Tracking", "Stale"], chart.Labels);
    }
}
