using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Web.BlazorComponents.Charts.ChartJs;

/// <summary>
/// Complete Chart.js configuration object passed to new Chart() constructor.
/// </summary>
/// <param name="Type">Chart type: "line", "bar", etc.</param>
/// <param name="Data">Chart data including labels and datasets.</param>
/// <param name="Options">Chart options including scales, plugins, etc.</param>
public record ChartJsConfig(
    string Type,
    ChartJsData Data,
    ChartJsOptions? Options = null
)
{
    /// <summary>
    /// Serializes to Chart.js compatible JSON using source-generated serializer.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, ChartJsJsonContext.Default.ChartJsConfig);
}

/// <summary>
/// Source-generated JSON serializer context for Chart.js types.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ChartJsConfig))]
[JsonSerializable(typeof(ChartJsData))]
[JsonSerializable(typeof(ChartJsOptions))]
public partial class ChartJsJsonContext : JsonSerializerContext;

/// <summary>
/// Extension methods for converting domain chart types to Chart.js config.
/// </summary>
public static class ChartJsConfigExtensions
{
    extension(Chart chart)
    {
        /// <summary>
        /// Converts a domain chart to a complete Chart.js configuration.
        /// </summary>
        /// <param name="options">Optional Chart.js options. Uses <see cref="ChartJsOptions.Default"/> if not specified.</param>
        public ChartJsConfig ToChartJsConfig(ChartJsOptions? options = null) => new(
            Type: chart.ToChartJsType(),
            Data: chart.ToChartJsData(),
            Options: options ?? ChartJsOptions.Default
        );
    }
}
