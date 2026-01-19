namespace AnimeFeedManager.Web.BlazorComponents.Charts.ChartJs;

/// <summary>
/// Provides mapping extensions from domain chart types to Chart.js data format.
/// </summary>
public static class ChartJsDataExtensions
{
    extension(Chart chart)
    {
        /// <summary>
        /// Maps any chart type to Chart.js data format.
        /// </summary>
        internal ChartJsData ToChartJsData() => chart switch
        {
            LineChart line => line.ToChartJsData(),
            BarChart bar => bar.ToChartJsData(),
            _ => throw new InvalidOperationException($"Unsupported chart type: {chart.GetType().Name}")
        };

        /// <summary>
        /// Determines the Chart.js chart type string.
        /// </summary>
        internal string ToChartJsType() => chart switch
        {
            Chart<LineDataset> => "line",
            Chart<BarDataset> => "bar",
            _ => throw new InvalidOperationException($"Unsupported chart type: {chart.GetType().Name}")
        };
    }

    extension(LineChart chart)
    {
        /// <summary>
        /// Maps a line chart to Chart.js data format.
        /// </summary>
        private ChartJsData ToChartJsData() => new(
            chart.Labels,
            chart.Datasets.Select(d => new ChartJsDataset(
                d.Label,
                d.Data,
                BorderColor: d.BorderColor.ToRgba(),
                Tension: d.Tension,
                Fill: d.Fill,
                BackgroundColor: d.BackgroundColor?.ToRgba()
            )).ToArray()
        );
    }

    extension(BarChart chart)
    {
        /// <summary>
        /// Maps a bar chart to Chart.js data format.
        /// </summary>
        private ChartJsData ToChartJsData() => new(
            chart.Labels,
            chart.Datasets.Select(d => new ChartJsDataset(
                d.Label,
                d.Data,
                BackgroundColor: d.BackgroundColor.ToRgba(),
                BorderColor: d.BorderColor?.ToRgba()
            )).ToArray()
        );
    }
}
