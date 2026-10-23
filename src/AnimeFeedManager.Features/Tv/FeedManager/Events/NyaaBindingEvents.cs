namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>Binds (or rebinds) a series to one nyaa release: group, release title and exact variant.</summary>
public sealed record BindNyaaSeries(
    string SeriesId,
    string Group,
    string ReleaseTitle,
    string Variant,
    bool Trusted) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "bind-nyaa-series";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.BindNyaaSeries);
    }
}

/// <summary>Fills a bound series' catalog from one nyaa search; never notifies.</summary>
public sealed record BackfillNyaaSeries(string SeriesId) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "backfill-nyaa-series";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.BackfillNyaaSeries);
    }
}
