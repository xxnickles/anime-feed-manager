namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Represents a date range for chart data filtering.
/// </summary>
internal readonly record struct ChartDateRange(DateTimeOffset From, DateTimeOffset To)
{
    /// <summary>
    /// Parses a period string (e.g., "7d", "30d") into a date range ending at the current time.
    /// </summary>
    /// <param name="period">Period string: "7d", "14d", "30d", "60d", "90d". Defaults to "30d" if invalid.</param>
    /// <returns>A date range from the calculated start date to now.</returns>
    public static ChartDateRange FromPeriod(string? period)
    {
        var now = DateTimeOffset.UtcNow;
        var days = ParseDays(period);
        return new ChartDateRange(now.AddDays(-days), now);
    }

    private static int ParseDays(string? period) => period switch
    {
        "7d" => 7,
        "14d" => 14,
        "30d" => 30,
        "60d" => 60,
        "90d" => 90,
        _ => 30 // Default to 30 days
    };
}
