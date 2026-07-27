namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

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

/// <summary>
/// Cross-partition query for every <see cref="ReleaseDetected"/> still <see cref="ReleaseDetectedStatus.Pending"/>
/// dispatch — the read side for the future notification-delivery process. <see cref="ReleaseDetected"/>
/// is partitioned by series id, so this necessarily fans out to every series partition; the
/// docType + status filter keeps the match set small regardless of library size, and the ~48h
/// <see cref="ReleaseDetected.Ttl"/> caps how long an entry can stay Pending before Cosmos removes it.
/// </summary>
public delegate Task<Result<ImmutableArray<ReleaseDetected>>> PendingReleaseDetectedLoader(
    CancellationToken cancellationToken);

/// <summary>Write-only — one document per job execution, read side is the observability trail.</summary>
public delegate Task<Result<Unit>> CollectionRunUpserter(
    CollectionRun run, CancellationToken cancellationToken);

/// <summary>
/// Most recent <paramref name="takePerSource"/> runs for each known <see cref="CollectionSource"/>,
/// concatenated (not globally capped — that's the caller's job when merging across buckets).
/// </summary>
public delegate Task<Result<ImmutableArray<CollectionRun>>> RecentCollectionRunsLoader(
    int takePerSource, CancellationToken cancellationToken);
