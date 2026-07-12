namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

public delegate Task<Result<Unit>> SeriesClassificationUpserter(
    SeriesClassification classification, CancellationToken cancellationToken);

/// <summary>Point-read by series id. Returns <c>NotFoundError</c> when never classified.</summary>
public delegate Task<Result<SeriesClassification>> SeriesClassificationLoader(
    int seriesId, CancellationToken cancellationToken);
