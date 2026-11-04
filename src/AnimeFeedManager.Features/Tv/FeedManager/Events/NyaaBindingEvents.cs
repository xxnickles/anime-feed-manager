using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>Binds (or rebinds) a series to one nyaa release: group, release title and exact variant.</summary>
/// <param name="RunId">Correlates the bind's result events with the run that sent it; a manual bind is a run of one.</param>
public sealed record BindNyaaSeries(
    string SeriesId,
    string Group,
    string ReleaseTitle,
    string Variant,
    bool Trusted,
    Guid RunId,
    string Origin = BindingOrigin.Manual) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "bind-nyaa-series";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.BindNyaaSeries);
    }
}

/// <summary>Fills a bound series' catalog from one nyaa search; never notifies.</summary>
/// <param name="Quiet">Sends no success event or app event; a failure still toasts.</param>
public sealed record BackfillNyaaSeries(string SeriesId, bool Quiet = false) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "backfill-nyaa-series";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.BackfillNyaaSeries);
    }
}
