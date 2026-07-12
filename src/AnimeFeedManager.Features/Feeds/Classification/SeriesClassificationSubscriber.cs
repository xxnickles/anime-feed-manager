using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Entities.Storage;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Classification;

/// <summary>
/// Reacts to <see cref="SeasonImported"/> — published by the existing Library import pipeline,
/// unmodified — to (re)classify every series in that season. Since the weekly import re-touches
/// the current season every run, this naturally re-classifies actively airing series on the same
/// cadence, with zero changes to Library's import code. Finished series are classified too, not
/// skipped: a batch/BD release can still land on Nyaa long after a series stops airing, so nothing
/// is ever treated as permanently "done" for tracking purposes.
/// </summary>
internal sealed class SeriesClassificationSubscriber(
    EventBus eventBus,
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    ILogger<SeriesClassificationSubscriber> logger) : IHostedService
{
    private readonly SeriesBySeasonLoader _loadSeason = cosmosFactory.SeriesBySeasonLoaderHandler();
    private readonly SeriesClassificationLoader _loadClassification = cosmosFactory.CosmosSeriesClassificationLoaderHandler();
    private readonly SeriesClassificationUpserter _upsertClassification = cosmosFactory.CosmosSeriesClassificationUpserterHandler();
    private IDisposable? _subscription;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscription = eventBus.Subscribe<SeasonImported>(HandleSeasonImported);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        return Task.CompletedTask;
    }

    private Task HandleSeasonImported(SeasonImported evt, CancellationToken cancellationToken) =>
        _loadSeason(evt.Season, cancellationToken).Match(
            series => ClassifyAll(series, cancellationToken),
            error => Task.Run(() => logger.LogWarning(
                "Skipping classification for season {Season}: {Error}", evt.Season, error.Message), cancellationToken));

    private Task ClassifyAll(ImmutableArray<Series> series, CancellationToken cancellationToken) =>
        Task.WhenAll(series.Select(s => ClassifyOne(s.MalId, cancellationToken)));

    // Trackability is monotonic (see SeriesClassifier) — load whatever we previously knew (absence
    // or any load error just means "no prior evidence", not a hard failure) so a fresh Jikan read
    // can only upgrade Untrackable -> Trackable, never the reverse.
    private async Task ClassifyOne(int malId, CancellationToken cancellationToken)
    {
        var previous = await _loadClassification(malId, cancellationToken);
        var previousTrackability = previous.MatchToValue(
            classification => classification.Trackability,
            _ => SeriesTrackability.Untrackable);

        await jikan.GetStreamingPlatforms(malId, cancellationToken)
            .Map(platforms => SeriesClassifier.Classify(malId, platforms, previousTrackability))
            .Bind(classification => _upsertClassification(classification, cancellationToken))
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to classify series {MalId}", malId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);
    }
}
