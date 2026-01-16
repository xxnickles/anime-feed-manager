namespace AnimeFeedManager.Shared.Charts;

/// <summary>
/// Color stored as RGBA components for Chart.js compatibility.
/// </summary>
/// <param name="Red">Red channel (0-255).</param>
/// <param name="Green">Green channel (0-255).</param>
/// <param name="Blue">Blue channel (0-255).</param>
/// <param name="Alpha">Opacity (0 = transparent, 1 = opaque).</param>
/// <remarks>
/// TODO: Consider migrating to OKLCH storage when Chart.js adds native support.
/// See: https://github.com/chartjs/Chart.js/issues/12101
/// OKLCH provides perceptual uniformity and wider gamut support.
/// </remarks>
public readonly record struct ChartColor(byte Red = 0, byte Green = 0, byte Blue = 0, float Alpha = 1f)
{   
    /// <summary>
    /// Converts the color to an RGB/RGBA string for Chart.js.
    /// </summary>
    public string ToRgba() => Alpha < 1f
        ? $"rgba({Red}, {Green}, {Blue}, {Alpha:F2})"
        : $"rgb({Red}, {Green}, {Blue})";
}
