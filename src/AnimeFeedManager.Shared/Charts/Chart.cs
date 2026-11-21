namespace AnimeFeedManager.Shared.Charts;

/// <summary>A series plotted against the shared category axis; <see cref="Data"/> aligns index-for-index with the chart's labels.</summary>
public sealed record CartesianDataset(string Label, float[] Data, ChartColor Color);

public closed record Chart(string[] Labels);

public sealed record LineChart(string[] Labels, CartesianDataset[] Datasets) : Chart(Labels);

/// <summary>Vertical bars, one group per label.</summary>
public sealed record ColumnChart(string[] Labels, CartesianDataset[] Datasets) : Chart(Labels);

public sealed record ChartSlice(string Label, float Value, ChartColor Color);

/// <summary>Parts of a whole, one slice per label.</summary>
public sealed record DonutChart(ChartSlice[] Slices) : Chart([.. Slices.Select(slice => slice.Label)]);
