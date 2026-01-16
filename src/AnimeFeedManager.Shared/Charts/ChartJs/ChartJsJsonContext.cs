using System.Text.Json.Serialization;

namespace AnimeFeedManager.Shared.Charts.ChartJs;

/// <summary>
/// Source-generated JSON serializer context for Chart.js data types.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ChartJsData))]
public partial class ChartJsJsonContext : JsonSerializerContext;
