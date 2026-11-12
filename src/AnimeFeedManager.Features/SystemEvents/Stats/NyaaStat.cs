using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record NyaaStats(int Automatic, int Manual)
{
    public int Bound => Automatic + Manual;
}

public static class NyaaStat
{
    /// <summary>Tracked bindings now, split by origin.</summary>
    public static Task<Result<NyaaStats, DomainError>> Get(TrackedNyaaBindings trackedBindings, CancellationToken cancellationToken) =>
        trackedBindings(cancellationToken).Map(Summarize);

    internal static NyaaStats Summarize(ImmutableArray<NyaaBindingStorage> tracked) =>
        new(tracked.Count(binding => binding.Origin == BindingOrigin.Schedule),
            tracked.Count(binding => binding.Origin != BindingOrigin.Schedule));
}
