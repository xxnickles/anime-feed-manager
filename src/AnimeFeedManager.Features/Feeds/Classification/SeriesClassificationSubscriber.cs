using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;
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
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    EventBus eventBus,
    ILogger<SeriesClassificationSubscriber> logger) : EventSubscriber<SeasonImported>
{
    private readonly SeriesBySeasonLoader _loadSeason = cosmosFactory.SeriesBySeasonLoaderHandler();

    private readonly SeriesClassificationLoader _loadClassification =
        cosmosFactory.CosmosSeriesClassificationLoaderHandler();

    private readonly SeriesClassificationUpserter _upsertClassification =
        cosmosFactory.CosmosSeriesClassificationUpserterHandler();

    private readonly FeedsOccurrenceUpserter _upsertOccurrence =
        cosmosFactory.CosmosFeedsOccurrenceUpserterHandler();

    public override Task Handle(SeasonImported evt, CancellationToken cancellationToken) =>
        _loadSeason(evt.Season, cancellationToken).Match(
            series => ClassifyAll(evt.Season, series, cancellationToken),
            error => Task.Run(() => logger.LogWarning(
                "Skipping classification for season {Season}: {Error}", evt.Season, error.Message), cancellationToken));

    // Back to simple sequential, one at a time (jikan-streaming's rate limiter is now a plain
    // 1/sec, no burst — see Jikan's registration). The persistent 504s that motivated the
    // burst/cooldown experiment turned out to be Jikan's "no data for this series" response, now
    // handled directly (see JikanClient.GetStreamingPlatforms) rather than paced around — this
    // simple baseline is to see whether a real 429 ever shows up under plain, conservative pacing.
    private async Task ClassifyAll(SeriesSeason season, ImmutableArray<Series> series,
        CancellationToken cancellationToken)
    {
        var degraded = 0;
        foreach (var s in series)
            if (await ClassifyOne(s.MalId, cancellationToken))
                degraded++;

        if (degraded > 0)
            await PersistDegradedOccurrence(degraded, series.Length, cancellationToken);
    }

    // Trackability is monotonic (see SeriesClassifier) — load whatever we previously knew (absence
    // or any load error just means "no prior evidence", not a hard failure) so a fresh Jikan read
    // can only upgrade Untrackable -> Trackable, never the reverse. Returns true when this series'
    // read was degraded (Jikan reported unavailable), for ClassifyAll to aggregate across the pass.
    private async Task<bool> ClassifyOne(int malId, CancellationToken cancellationToken)
    {
        var previous = await _loadClassification(malId, cancellationToken);
        var previousTrackability = previous.MatchToValue(
            classification => classification.Trackability,
            _ => SeriesTrackability.Untrackable);
        var previousPlatforms = previous.MatchToValue(
            classification => classification.Platforms,
            _ => null!);

        var platformsResult = await jikan.GetStreamingPlatforms(malId, cancellationToken);
        var degraded = platformsResult.MatchToValue(_ => false, error => error is JikanUnavailableError);

        await platformsResult
            // Jikan's 504 is a known, recurring "unavailable" signal — recover to an empty read
            // rather than failing classification outright; SeriesClassifier's own monotonic guard
            // keeps an empty read from erasing previously-known platforms.
            .BindOnErrorWhen(
                binder: _ => ImmutableArray<JikanStreamingEntry>.Empty,
                predicate: error => error is JikanUnavailableError)
            .Map(platforms => SeriesClassifier.Classify(malId, platforms, previousTrackability, previousPlatforms))
            .Bind(classification => _upsertClassification(classification, cancellationToken))
            .AddLogOnSuccess(_ => log => log.LogInformation("Classified series {MalId}", malId))
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to classify series {MalId}", malId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);

        return degraded;
    }

    // One summary occurrence per classification pass, not one per series — persisted for the
    // activity feed and published live for the admin toast. Best-effort: a failure to persist
    // doesn't fail classification.
    private async Task PersistDegradedOccurrence(int degraded, int total, CancellationToken cancellationToken)
    {
        var occurrence = new FeedsOccurrence(FeedsSources.Classification)
        {
            Kind = "jikan-unavailable",
            Outcome = Outcome.Warning,
            Summary = $"{degraded} of {total} series not available in Jikan",
            OccurredAt = DateTimeOffset.UtcNow
        };
        eventBus.Publish(occurrence);

        await _upsertOccurrence(occurrence, cancellationToken)
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to persist jikan-unavailable feeds occurrence"))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);
    }
}