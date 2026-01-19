namespace AnimeFeedManager.Web.BlazorComponents.Charts.ChartJs;

/// <summary>
/// Chart.js data contract. Use <see cref="ToJson"/> for serialization.
/// </summary>
/// <param name="Labels">X-axis labels.</param>
/// <param name="Datasets">One or more datasets to display.</param>
public record ChartJsData(string[] Labels, ChartJsDataset[] Datasets);

/// <summary>
/// Chart.js dataset contract. Null properties are omitted during serialization.
/// </summary>
/// <param name="Label">Display name in the chart legend.</param>
/// <param name="Data">Data points corresponding to each label.</param>
/// <param name="BorderColor">Line/bar border color (RGBA string).</param>
/// <param name="BackgroundColor">Fill/bar color (RGBA string).</param>
/// <param name="Tension">Line smoothing (0 = straight, 0.3-0.4 = smooth). Line charts only.</param>
/// <param name="Fill">Whether to fill area under line. Line charts only.</param>
public record ChartJsDataset(
    string Label,
    float[] Data,
    string? BorderColor = null,
    string? BackgroundColor = null,
    float? Tension = null,
    bool? Fill = null
);
