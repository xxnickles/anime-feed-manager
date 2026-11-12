using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

public static class NyaaWindow
{
    /// <summary>The last stamped poll time; null before the first stamped run.</summary>
    public static Task<Result<DateTimeOffset?, DomainError>> LastPoll(LastNyaaPoll pollGetter, CancellationToken token) =>
        pollGetter(token)
            .Map(DateTimeOffset? (poll) => poll.At)
            .BindOnErrorWhen(_ => Result<DateTimeOffset?, DomainError>.Success(null), error => error is NotFoundError);

    /// <summary>Releases may have fallen out of the 75-item window: the oldest polled one is newer than the last poll. No stored poll is no gap.</summary>
    internal static bool HasGap(DateTimeOffset? lastPoll, DateTimeOffset? oldestPolled) =>
        lastPoll is { } last && oldestPolled is { } oldest && oldest > last;

    extension(Task<Result<NyaaCollected, DomainError>> collected)
    {
        /// <summary>On a gap, queues a paced backfill of every tracked binding with a warning; the run goes on either way.</summary>
        public Task<Result<NyaaCollected, DomainError>> BackfillOnGap(
            DateTimeOffset? lastPoll,
            DelayedMessageSender delayedSender,
            CancellationToken token) =>
            collected.Bind(c => HasGap(lastPoll, c.OldestPolled)
                ? Backfill(c.TrackedSeries, delayedSender, token)
                    .Map(_ => c)
                    .AddLogOnSuccess(_ => GapWarning(lastPoll, c))
                : Task.FromResult(Result<NyaaCollected, DomainError>.Success(c)));
    }

    private static async Task<Result<int, DomainError>> Backfill(
        ImmutableArray<string> seriesIds,
        DelayedMessageSender delayedSender,
        CancellationToken token)
    {
        var sends = await Task.WhenAll(seriesIds.Select((seriesId, index) =>
            delayedSender(new BackfillNyaaSeries(seriesId), NyaaPacing.For(index), token)));

        return sends
            .Flatten(sent => sent.Count())
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => bulk.Value);
    }

    private static Action<ILogger> GapWarning(DateTimeOffset? lastPoll, NyaaCollected collected) =>
        logger => logger.LogWarning(
            "Nyaa poll window gap: oldest polled release {nyaa.oldest_published} is newer than the last poll {nyaa.last_poll}; backfilling {nyaa.backfill_count} tracked bindings",
            collected.OldestPolled, lastPoll, collected.TrackedSeries.Length);
}
