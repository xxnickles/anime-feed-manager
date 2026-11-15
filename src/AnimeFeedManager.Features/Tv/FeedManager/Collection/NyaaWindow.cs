using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

public static class NyaaWindow
{
    /// <summary>The last stamped poll time; null before the first stamped run.</summary>
    public static Task<Result<DateTimeOffset?, StorageError>> LastPoll(LastNyaaPoll pollGetter, CancellationToken token) =>
        pollGetter(token)
            .Map(DateTimeOffset? (poll) => poll.At)
            .BindOnErrorWhen(_ => Result<DateTimeOffset?, StorageError>.Success(null), error => error is StorageError.NotFound);

    /// <summary>Releases may have fallen out of the 75-item window: the oldest polled one is newer than the last poll. No stored poll is no gap.</summary>
    internal static bool HasGap(DateTimeOffset? lastPoll, DateTimeOffset? oldestPolled) =>
        lastPoll is { } last && oldestPolled is { } oldest && oldest > last;

    /// <summary>On a gap, queues a paced backfill of every tracked binding with a warning; fails only when no backfill could be queued.</summary>
    public static Task<Result<NyaaCollected, DeliveryError>> BackfillOnGap(
        NyaaCollected collected,
        DateTimeOffset? lastPoll,
        DelayedMessageSender delayedSender,
        CancellationToken token) =>
        HasGap(lastPoll, collected.OldestPolled)
            ? Backfill(collected.TrackedSeries, delayedSender, token)
                .Map(_ => collected)
                .AddLogOnSuccess(_ => GapWarning(lastPoll, collected))
            : Task.FromResult(Result<NyaaCollected, DeliveryError>.Success(collected));

    private static async Task<Result<int, DeliveryError>> Backfill(
        ImmutableArray<string> seriesIds,
        DelayedMessageSender delayedSender,
        CancellationToken token)
    {
        var sends = await Task.WhenAll(seriesIds.Select((seriesId, index) =>
            delayedSender(new BackfillNyaaSeries(seriesId), NyaaPacing.For(index), token)));

        return sends
            .Collect(sent => sent.Count(),
                DeliveryError (errors) => new DeliveryError.BatchFailed("No tracked binding's backfill could be queued", errors))
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => bulk.Value);
    }

    private static Action<ILogger> GapWarning(DateTimeOffset? lastPoll, NyaaCollected collected) =>
        logger => logger.LogWarning(
            "Nyaa poll window gap: oldest polled release {nyaa.oldest_published} is newer than the last poll {nyaa.last_poll}; backfilling {nyaa.backfill_count} tracked bindings",
            collected.OldestPolled, lastPoll, collected.TrackedSeries.Length);
}
