using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Feeds.Storage;

public delegate Task<Result<Unit>> SeriesClassificationUpserter(
    SeriesClassification classification, CancellationToken cancellationToken);

/// <summary>Point-read by series id. Returns <c>NotFoundError</c> when never classified.</summary>
public delegate Task<Result<SeriesClassification>> SeriesClassificationLoader(
    int seriesId, CancellationToken cancellationToken);

public delegate Task<Result<Unit>> CollectionCheckpointUpserter(
    CollectionCheckpoint checkpoint, CancellationToken cancellationToken);

/// <summary>Point-read by source. Returns <c>NotFoundError</c> when the source has never run.</summary>
public delegate Task<Result<CollectionCheckpoint>> CollectionCheckpointLoader(
    CollectionSource source, CancellationToken cancellationToken);

public delegate Task<Result<Unit>> NyaaConfirmationUpserter(
    NyaaConfirmation confirmation, CancellationToken cancellationToken);

/// <summary>Point-read by series id. Returns <c>NotFoundError</c> when never confirmed.</summary>
public delegate Task<Result<NyaaConfirmation>> NyaaConfirmationLoader(
    int seriesId, CancellationToken cancellationToken);

public delegate Task<Result<Unit>> AiringClockFlagUpserter(
    AiringClockFlag flag, CancellationToken cancellationToken);

/// <summary>Point-read by series id. Returns <c>NotFoundError</c> when never flagged.</summary>
public delegate Task<Result<AiringClockFlag>> AiringClockFlagLoader(
    int seriesId, CancellationToken cancellationToken);

public delegate Task<Result<Unit>> ReleaseDetectedUpserter(
    ReleaseDetected release, CancellationToken cancellationToken);

/// <summary>Write-only — one document per job execution, read side is the observability trail.</summary>
public delegate Task<Result<Unit>> CollectionRunUpserter(
    CollectionRun run, CancellationToken cancellationToken);

/// <summary>
/// Most recent <paramref name="takePerSource"/> runs for each known <see cref="CollectionSource"/>,
/// concatenated (not globally capped — that's the caller's job when merging across buckets).
/// </summary>
public delegate Task<Result<ImmutableArray<CollectionRun>>> RecentCollectionRunsLoader(
    int takePerSource, CancellationToken cancellationToken);

/// <summary>Write-only — one document per notable occurrence, read side is the admin activity feed.</summary>
public delegate Task<Result<Unit>> FeedsOccurrenceUpserter(
    FeedsOccurrence occurrence, CancellationToken cancellationToken);

/// <summary>Most recent <paramref name="take"/> occurrences, newest first.</summary>
public delegate Task<Result<ImmutableArray<FeedsOccurrence>>> RecentFeedsOccurrencesLoader(
    int take, CancellationToken cancellationToken);

/// <summary>Subscribes a user to a series — plain create, idempotent (re-subscribing just re-upserts).</summary>
public delegate Task<Result<Unit>> SeriesSubscriberUpserter(
    SeriesSubscriber subscriber, CancellationToken cancellationToken);

/// <summary>Unsubscribes a user from a series — plain delete; already-unsubscribed is a no-op success.</summary>
public delegate Task<Result<Unit>> SeriesSubscriberRemover(
    int seriesId, string userId, CancellationToken cancellationToken);

/// <summary>Every subscriber of a series — single-partition query, no cross-partition fan-out.</summary>
public delegate Task<Result<ImmutableArray<SeriesSubscriber>>> SeriesSubscribersLoader(
    int seriesId, CancellationToken cancellationToken);
