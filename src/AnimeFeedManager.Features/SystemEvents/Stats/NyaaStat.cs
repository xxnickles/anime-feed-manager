using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record NyaaStats(int Automatic, int Manual)
{
    public int Bound => Automatic + Manual;
}

public static class NyaaStat
{
    public static Task<Result<NyaaStats>> Get(
        SystemEventsGetter<NyaaSeriesBound> boundGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken) =>
        boundGetter(TargetConsumer.Admin(), from, cancellationToken).Map(Summarize);

    internal static NyaaStats Summarize(ImmutableArray<EventData<NyaaSeriesBound>> bound) =>
        new(bound.Count(e => e.Data.Origin == BindingOrigin.Schedule),
            bound.Count(e => e.Data.Origin != BindingOrigin.Schedule));
}
