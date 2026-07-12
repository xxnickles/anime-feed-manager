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

/// <summary>Write-only — read side belongs to the future notification-delivery process.</summary>
public delegate Task<Result<Unit>> ReleaseDetectedUpserter(
    ReleaseDetected release, CancellationToken cancellationToken);

/// <summary>Write-only — one document per job execution, read side is the observability trail.</summary>
public delegate Task<Result<Unit>> CollectionRunUpserter(
    CollectionRun run, CancellationToken cancellationToken);
