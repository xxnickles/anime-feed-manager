using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Staleness;

public static class NyaaStaleness
{
    public static readonly TimeSpan Threshold = TimeSpan.FromDays(14);

    /// <summary>A tracked, uncompleted binding whose newest release is at least <see cref="Threshold"/> old; an empty catalog is never stale.</summary>
    public static bool IsStale(NyaaBindingStorage binding, IEnumerable<NyaaCatalogStorage> catalog, DateTimeOffset now) =>
        binding is {Tracking: true, CompletedAt: null}
        && catalog.Max(row => (DateTimeOffset?) row.Published) is { } newest
        && newest <= now - Threshold;
}
