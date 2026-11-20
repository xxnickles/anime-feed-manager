using AnimeFeedManager.Shared.Charts;
using AnimeFeedManager.Web.BlazorComponents.Charts.Apex;

namespace AnimeFeedManager.Web.BlazorComponents.Tests.Charts.Apex;

public class ApexPayloadTests
{
    #region Kind

    [Fact]
    public void Should_Use_Line_Kind_When_Chart_Is_LineChart()
    {
        var payload = new LineChart(["Nov 01"], [Series("Completed", ChartColor.Success, 1)]).ToApexPayload();

        Assert.Equal("line", payload.Kind);
    }

    [Fact]
    public void Should_Use_Column_Kind_When_Chart_Is_ColumnChart()
    {
        var payload = new ColumnChart(["Nov 01"], [Series("Sent", ChartColor.BaseContent, 1)]).ToApexPayload();

        Assert.Equal("column", payload.Kind);
    }

    #endregion

    #region Data

    [Fact]
    public void Should_Serialize_Categories_And_Series_When_Chart_Has_Several_Datasets()
    {
        var chart = new LineChart(
            ["Nov 01", "Nov 02"],
            [Series("Completed", ChartColor.Success, 3, 1), Series("Errors", ChartColor.Error, 0, 2)]);

        var payload = chart.ToApexPayload();

        Assert.Equal(
            """{"categories":["Nov 01","Nov 02"],"series":[{"name":"Completed","data":[3,1],"color":"success"},{"name":"Errors","data":[0,2],"color":"error"}]}""",
            payload.Data);
    }

    [Fact]
    public void Should_Serialize_Column_Chart_With_Same_Shape_As_Line_Chart()
    {
        var chart = new ColumnChart(["Nov 01"], [Series("Notifications Sent", ChartColor.BaseContent, 4)]);

        var payload = chart.ToApexPayload();

        Assert.Equal(
            """{"categories":["Nov 01"],"series":[{"name":"Notifications Sent","data":[4],"color":"base-content"}]}""",
            payload.Data);
    }

    [Fact]
    public void Should_Serialize_Empty_Arrays_When_Chart_Has_No_Labels_Or_Datasets()
    {
        var payload = new LineChart([], []).ToApexPayload();

        Assert.Equal("""{"categories":[],"series":[]}""", payload.Data);
    }

    #endregion

    #region Test Helpers

    private static CartesianDataset Series(string label, ChartColor color, params float[] data) =>
        new(label, data, color);

    #endregion
}
