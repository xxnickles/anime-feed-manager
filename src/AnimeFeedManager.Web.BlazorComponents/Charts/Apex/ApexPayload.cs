using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Web.BlazorComponents.Charts.Apex;

/// <summary>What an <c>&lt;afm-chart&gt;</c> receives: a preset name and its JSON data; styling lives in the element's presets.</summary>
/// <remarks>Kinds are allowlisted client-side, so add a kind on both sides.</remarks>
public sealed record ApexPayload(string Kind, string Data);

public sealed record ApexSeries(string Name, float[] Data, string Color);

public sealed record ApexCartesianData(string[] Categories, ApexSeries[] Series);

public sealed record ApexSlice(string Name, float Value, string Color);

public sealed record ApexSliceData(ApexSlice[] Slices);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ApexCartesianData))]
[JsonSerializable(typeof(ApexSliceData))]
internal partial class ApexJsonContext : JsonSerializerContext;

public static class ApexPayloadExtensions
{
    extension(Chart chart)
    {
        public ApexPayload ToApexPayload() => chart switch
        {
            LineChart line => new ApexPayload("line", Cartesian(line.Labels, line.Datasets)),
            ColumnChart column => new ApexPayload("column", Cartesian(column.Labels, column.Datasets)),
            DonutChart donut => new ApexPayload("donut", Slices(donut.Slices))
        };
    }

    private static string Cartesian(string[] labels, CartesianDataset[] datasets) =>
        JsonSerializer.Serialize(
            new ApexCartesianData(labels, [..datasets.Select(d => new ApexSeries(d.Label, d.Data, d.Color.Token))]),
            ApexJsonContext.Default.ApexCartesianData);

    private static string Slices(ChartSlice[] slices) =>
        JsonSerializer.Serialize(
            new ApexSliceData([..slices.Select(s => new ApexSlice(s.Label, s.Value, s.Color.Token))]),
            ApexJsonContext.Default.ApexSliceData);
}
