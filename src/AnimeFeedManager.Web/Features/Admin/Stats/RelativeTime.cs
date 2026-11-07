namespace AnimeFeedManager.Web.Features.Admin.Stats;

internal static class RelativeTime
{
    /// <summary>Compact elapsed time from now: <c>5m ago</c>, <c>3h ago</c>, <c>2d ago</c>.</summary>
    public static string Ago(DateTimeOffset at)
    {
        var elapsed = DateTimeOffset.UtcNow - at;
        return elapsed switch
        {
            { TotalMinutes: < 60 } => $"{Math.Max(1, (int)elapsed.TotalMinutes)}m ago",
            { TotalHours: < 24 } => $"{(int)elapsed.TotalHours}h ago",
            _ => $"{(int)elapsed.TotalDays}d ago"
        };
    }
}
