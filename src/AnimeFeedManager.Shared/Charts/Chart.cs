namespace AnimeFeedManager.Shared.Charts;

/// <summary>
/// Base record for chart datasets. All dataset types share a label and data points.
/// </summary>
/// <param name="Label">Display name for this dataset in the chart legend.</param>
/// <param name="Data">Data points corresponding to each label in the chart.</param>
public abstract record Dataset(string Label, float[] Data);

/// <summary>
/// Non-generic base for all chart types. Enables pattern matching on chart instances.
/// </summary>
/// <param name="Labels">X-axis labels shared by all datasets.</param>
public abstract record Chart(string[] Labels);

/// <summary>
/// Generic chart base providing typed access to datasets.
/// </summary>
/// <typeparam name="TDataset">The dataset type this chart contains.</typeparam>
/// <param name="Labels">X-axis labels shared by all datasets.</param>
/// <param name="Datasets">One or more datasets to display on the chart.</param>
public abstract record Chart<TDataset>(string[] Labels, TDataset[] Datasets)
    : Chart(Labels) where TDataset : Dataset;

/// <summary>
/// Line chart dataset with line-specific styling options.
/// </summary>
/// <param name="Label">Display name for this dataset in the chart legend.</param>
/// <param name="Data">Data points corresponding to each label in the chart.</param>
/// <param name="BorderColor">Line color.</param>
/// <param name="Tension">Line smoothing. 0 = straight lines, 0.3-0.4 = smooth curves.</param>
/// <param name="Fill">Whether to fill the area under the line.</param>
/// <param name="BackgroundColor">Fill color when Fill is true. Uses BorderColor with transparency if not specified.</param>
public record LineDataset(
    string Label,
    float[] Data,
    ChartColor BorderColor,
    float Tension = 0.3f,
    bool Fill = false,
    ChartColor? BackgroundColor = null
) : Dataset(Label, Data);

/// <summary>
/// Bar chart dataset with bar-specific styling options.
/// </summary>
/// <param name="Label">Display name for this dataset in the chart legend.</param>
/// <param name="Data">Data points corresponding to each label in the chart.</param>
/// <param name="BackgroundColor">Bar fill color.</param>
/// <param name="BorderColor">Bar outline color. Optional.</param>
public record BarDataset(
    string Label,
    float[] Data,
    ChartColor BackgroundColor,
    ChartColor? BorderColor = null
) : Dataset(Label, Data);

/// <summary>
/// Line chart containing one or more line datasets.
/// </summary>
/// <param name="Labels">X-axis labels shared by all datasets.</param>
/// <param name="Datasets">One or more line datasets to display on the chart.</param>
public record LineChart(string[] Labels, LineDataset[] Datasets)
    : Chart<LineDataset>(Labels, Datasets);

/// <summary>
/// Bar chart containing one or more bar datasets.
/// </summary>
/// <param name="Labels">X-axis labels shared by all datasets.</param>
/// <param name="Datasets">One or more bar datasets to display on the chart.</param>
public record BarChart(string[] Labels, BarDataset[] Datasets)
    : Chart<BarDataset>(Labels, Datasets);
