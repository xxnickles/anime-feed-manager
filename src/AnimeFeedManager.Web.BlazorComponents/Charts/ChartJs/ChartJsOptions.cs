namespace AnimeFeedManager.Web.BlazorComponents.Charts.ChartJs;

/// <summary>
/// Chart.js options contract. Null properties are omitted during serialization.
/// </summary>
/// <param name="Responsive">Whether the chart resizes with its container. Default: true.</param>
/// <param name="MaintainAspectRatio">Whether to maintain aspect ratio when resizing. Default: false.</param>
/// <param name="Plugins">Plugin configuration (legend, tooltip, etc.).</param>
/// <param name="Scales">Axis scale configuration. Null by default (opt-in).</param>
public record ChartJsOptions(
    bool Responsive = true,
    bool MaintainAspectRatio = false,
    ChartJsPlugins? Plugins = null,
    ChartJsScales? Scales = null,
    ChartJsAnimation? Animation = null
)
{
    /// <summary>
    /// Default options with legend at bottom. Animation uses Chart.js defaults.
    /// </summary>
    public static ChartJsOptions Default => new(
        Plugins: new ChartJsPlugins(Legend: new ChartJsLegend())
    );

    /// <summary>
    /// Options configured for integer Y-axis values (counts, quantities).
    /// </summary>
    public static ChartJsOptions IntegerScale => Default with
    {
        Scales = new ChartJsScales(Y: new ChartJsAxis(Ticks: new ChartJsAxisTicks(Precision: 0)))
    };
}

/// <summary>
/// Chart.js plugins configuration.
/// </summary>
/// <param name="Legend">Legend display configuration.</param>
public record ChartJsPlugins(
    ChartJsLegend? Legend = null
);

/// <summary>
/// Chart.js legend configuration.
/// </summary>
/// <param name="Position">Legend position: "top", "bottom", "left", "right".</param>
public record ChartJsLegend(
    string Position = "bottom"
);

/// <summary>
/// Chart.js scales configuration for X and Y axes.
/// </summary>
/// <param name="X">X-axis configuration.</param>
/// <param name="Y">Y-axis configuration.</param>
public record ChartJsScales(
    ChartJsAxis? X = null,
    ChartJsAxis? Y = null
);

/// <summary>
/// Chart.js axis configuration.
/// </summary>
/// <param name="Ticks">Tick mark configuration.</param>
/// <param name="BeginAtZero">Whether the axis should start at zero.</param>
public record ChartJsAxis(
    ChartJsAxisTicks? Ticks = null,
    bool? BeginAtZero = null
);

/// <summary>
/// Chart.js axis tick configuration.
/// </summary>
/// <param name="Precision">Number of decimal places. Use 0 for integers.</param>
/// <param name="StepSize">Fixed step size between ticks.</param>
public record ChartJsAxisTicks(
    int? Precision = null,
    int? StepSize = null
);

/// <summary>
/// Chart.js animation configuration.
/// </summary>
/// <param name="Duration">Animation duration in milliseconds. Default is 1000. Use 0 to disable.</param>
/// <param name="Easing">Easing function: "linear", "easeInQuad", "easeOutQuad", "easeInOutQuad",
/// "easeInCubic", "easeOutCubic", "easeInOutCubic", "easeInQuart", "easeOutQuart", "easeInOutQuart",
/// "easeInQuint", "easeOutQuint", "easeInOutQuint", "easeInSine", "easeOutSine", "easeInOutSine",
/// "easeInExpo", "easeOutExpo", "easeInOutExpo", "easeInCirc", "easeOutCirc", "easeInOutCirc",
/// "easeInElastic", "easeOutElastic", "easeInOutElastic", "easeInBack", "easeOutBack", "easeInOutBack",
/// "easeInBounce", "easeOutBounce", "easeInOutBounce".</param>
/// <param name="Delay">Delay before animation starts in milliseconds.</param>
public record ChartJsAnimation(
    int? Duration = null,
    string? Easing = null,
    int? Delay = null
);
