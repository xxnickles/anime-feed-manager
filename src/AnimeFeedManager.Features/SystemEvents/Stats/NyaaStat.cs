using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record TruncatedBackfill(string SeriesId, string Title, DateTimeOffset At);

public sealed record NyaaStats(int Bound, int Backfilled, ImmutableArray<TruncatedBackfill> PossiblyTruncated);

public static class NyaaStat
{
    public static Task<Result<NyaaStats>> Get(
        SystemEventsGetter<NyaaSeriesBound> boundGetter,
        SystemEventsGetter<NyaaSeriesBackfilled> backfilledGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken)
    {
        var consumer = TargetConsumer.Admin();
        return boundGetter(consumer, from, cancellationToken)
            .Bind(bound => backfilledGetter(consumer, from, cancellationToken)
                .Map(backfilled => Summarize(bound, backfilled)));
    }

    // Only a series' latest backfill counts: a later full catalog clears an earlier truncation.
    internal static NyaaStats Summarize(
        ImmutableArray<EventData<NyaaSeriesBound>> bound,
        ImmutableArray<EventData<NyaaSeriesBackfilled>> backfilled) =>
        new(bound.Length,
            backfilled.Length,
            [
                .. backfilled
                    .GroupBy(e => e.Data.SeriesId)
                    .Select(g => g.MaxBy(e => e.Timestamp)!)
                    .Where(e => e.Data.PossiblyTruncated)
                    .OrderByDescending(e => e.Timestamp)
                    .Select(e => new TruncatedBackfill(e.Data.SeriesId, e.Data.Title, e.Timestamp))
            ]);
}
