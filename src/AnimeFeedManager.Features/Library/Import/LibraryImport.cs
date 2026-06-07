using System.Diagnostics;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Mappers;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;

namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Library import: fetches a season from Jikan and, for each series, stores its cover in blob
/// storage then persists the series to Cosmos with the stored blob path, finally upserting the
/// seasons index. Cover upload is best-effort — a failure keeps the source URL, and a re-import
/// retries it. Run as a background job via <c>JobExecutor</c>, triggered on a schedule by
/// <see cref="LibraryImportCronJob"/>.
/// </summary>
internal static class LibraryImport
{
    private static readonly ActivitySource Source = new(Telemetry.LibraryImportSource);

    public static async Task<Result<Unit>> Execute(
        ImportTarget target,
        IJikanClient jikan,
        SingleSeriesPersistenceHandler<CosmosOperationCost> persistSeries,
        LibrarySeasonsIndexUpserter upsertIndex,
        SeriesImageProcessor processImage,
        TimeProvider time,
        CancellationToken cancellationToken)
    {
        using var importActivity = Source.StartActivity("Library.Import");
        importActivity?.SetTag("library.import.target", target.GetType().Name);

        var source = target switch
        {
            ImportTarget.CurrentSeason => jikan.GetCurrentSeason(cancellationToken),
            ImportTarget.SpecificSeason s => jikan.GetSeason(s.SeriesSeason.Year, s.SeriesSeason.Season,
                cancellationToken),
            _ => throw new UnreachableException()
        };

        var now = time.GetUtcNow();
        return await PersistSeries(source, now, persistSeries, processImage, cancellationToken)
            .Tap(result => SetImportActivityTags(importActivity, result))
            .AddLogOnSuccess(LogFactories.Log<ImportResult>((result, iLogger) =>
                iLogger.LogInformation("Imported {Imported} series", result.Imported)))
            .Bind(result => upsertIndex(new SeasonEntry(result.SeriesSeason, now, result.Imported), cancellationToken))
            .Map(_ => new Unit())
            .MarkActivityErroredOnError();
    }

    private static async Task<Result<ImportResult>> PersistSeries(
        IAsyncEnumerable<Result<JikanPage>> source,
        DateTimeOffset now,
        SingleSeriesPersistenceHandler<CosmosOperationCost> seriesPersistenceHandler,
        SeriesImageProcessor processImage,
        CancellationToken cancellationToken
    )
    {
        var accumulated = new List<Result<ImportResult>>();
        var pageIndex = 0;

        await foreach (var pageResult in source.WithCancellation(cancellationToken))
        {
            pageIndex++;
            using var pageActivity = Source.StartActivity("Library.Import.Page");
            pageActivity?.SetTag("library.import.page.index", pageIndex);

            var persistResult =
                await pageResult
                    .Tap(page => pageActivity?.SetTag("library.import.page.input_count", page.Items.Length))
                    .Bind(page => ProcessPage(page, seriesPersistenceHandler, processImage, now, cancellationToken))
                    .Tap(bulkResult => SetPageActivityTags(pageActivity, bulkResult))
                    // Partial errors become a full error as they are only possible for recoverable cases
                    .Bind(results =>
                    {
                        return results switch
                        {
                            CompletedBulkResult<ImportResult> completedBulkResult =>
                                Result<ImportResult>.Success(completedBulkResult.Value),
                            PartialSuccessBulkResult<ImportResult> partialBulkResult => new
                                AggregatedError(
                                    $"Page had {partialBulkResult.Errors.Length} transient errors; retrying.",
                                    partialBulkResult.Errors),
                            _ => throw new UnreachableException()
                        };
                    })
                    .MarkActivityErroredOnError();

            accumulated.AddRange(persistResult);
            // If there was full error, stop processing
            if (persistResult.IsFailure)
                break;
        }

        return accumulated
            .Flatten(results => results.Aggregate((first, next) => first with
            {
                Imported = first.Imported + next.Imported,
                TotalCost = first.TotalCost + next.TotalCost
            }))
            .Bind(results => results switch
            {
                CompletedBulkResult<ImportResult> completed => Result<ImportResult>.Success(completed.Value),
                PartialSuccessBulkResult<ImportResult> partial => new AggregatedError(
                    "Partial success during import; will retry", partial.Errors),
                _ => throw new UnreachableException()
            });
    }

    private static async Task<Result<BulkResult<ImportResult>>> ProcessPage(
        JikanPage page,
        SingleSeriesPersistenceHandler<CosmosOperationCost> persistSeries,
        SeriesImageProcessor processImage,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // Jikan occasionally lists the same series twice on a page; dedup by MAL id so a series is
        // never processed (nor concurrently upserted) twice in one run — concurrent upserts of a
        // not-yet-existing document race, and the loser gets a 409 Conflict.
        var distinct = page.Items.DistinctBy(item => item.MalId).ToArray();
        Activity.Current?.SetTag("library.import.page.duplicates_removed", page.Items.Length - distinct.Length);

        var results = await Task.WhenAll(distinct.Select(item =>
            ProcessItem(item, page.Season, now, persistSeries, processImage, cancellationToken)));

        return results.Flatten(charges => Map(page.Season, charges));
    }

    // One series under its own span (tagged with mal_id) so every blob/cosmos call for this series
    // nests beneath it — the trace groups by series, and by function within (Cover, Persist).
    // Permanent failures (parse/validation, non-transient Cosmos) are skipped: logged, recorded on
    // the span as outcome=skipped + a series.skipped event, and turned into a zero-cost success so
    // they don't poison the page into a retry. Skips are an expected outcome, not a span error.
    private static async Task<Result<CosmosOperationCost>> ProcessItem(
        JikanAnime item,
        SeriesSeason season,
        DateTimeOffset now,
        SingleSeriesPersistenceHandler<CosmosOperationCost> persistSeries,
        SeriesImageProcessor processImage,
        CancellationToken cancellationToken)
    {
        using var seriesActivity = Source.StartActivity("Library.Import.Series");
        seriesActivity?.SetTag("library.import.series.mal_id", item.MalId);

        return await item.ToSeries(season, now)
            .AddLogOnFailure(_ => logger => logger.LogWarning(
                "Failed to parse series from Jikan. Item {Id} {Series}",
                item.MalId,
                JsonSerializer.Serialize(item, JikanJsonContext.Default.JikanAnime)))
            .Tap(parsed => seriesActivity?
                .SetTag("library.import.series.title", parsed.Titles.Default)
                .SetTag("library.import.series.type", parsed.GetType().Name))
            // Store the cover first, then persist the series carrying the stored blob path.
            .Bind(parsed => WithStoredCover(parsed, processImage, seriesActivity, cancellationToken)
                .Bind(withCover => Persist(withCover, persistSeries, seriesActivity, cancellationToken)))
            .BindOnErrorWhen(
                binder: error =>
                {
                    seriesActivity?
                        .SetTag("library.import.series.outcome", "skipped")
                        .SetTag("library.import.series.skip_reason", error.GetType().Name);
                    seriesActivity?.AddEvent(new ActivityEvent("series.skipped",
                        tags: new ActivityTagsCollection { { "reason", error.Message } }));
                    return new CosmosOperationCost(0);
                },
                predicate: IsPermanent);
    }

    // Best-effort cover storage under a "Cover" span: upload the cover and return the series with
    // the stored blob path. A null source URL skips upload (cover=none); an upload failure is logged
    // and recovers to the original series (keeping the Jikan URL, cover=fallback) so cover trouble
    // never fails the persist. Re-imports retry via the processor's blob-exists idempotency.
    private static async Task<Result<Series>> WithStoredCover(
        Series parsed,
        SeriesImageProcessor processImage,
        Activity? seriesActivity,
        CancellationToken cancellationToken)
    {
        if (parsed.CoverImageUrl is not { } coverUrl)
        {
            seriesActivity?.SetTag("library.import.series.cover", "none");
            return Result<Series>.Success(parsed);
        }

        using var coverActivity = Source.StartActivity("Library.Import.Series.Cover");

        return await processImage(parsed.Id, parsed.SeriesSeason, coverUrl, cancellationToken)
            .Map(blobPath => parsed with { CoverImageUrl = blobPath })
            .Tap(_ => seriesActivity?.SetTag("library.import.series.cover", "stored"))
            .AddLogOnFailure(_ => logger => logger.LogWarning(
                "Failed to store cover for series {Id}; keeping source URL", parsed.Id))
            .BindOnErrorWhen(
                binder: _ =>
                {
                    seriesActivity?.SetTag("library.import.series.cover", "fallback");
                    return parsed;
                },
                predicate: _ => true);
    }

    // Persist under a "Persist" span; on success the series span's outcome is "persisted".
    private static async Task<Result<CosmosOperationCost>> Persist(
        Series withCover,
        SingleSeriesPersistenceHandler<CosmosOperationCost> persistSeries,
        Activity? seriesActivity,
        CancellationToken cancellationToken)
    {
        using var persistActivity = Source.StartActivity("Library.Import.Series.Persist");

        return await persistSeries(withCover, cancellationToken)
            .Tap(_ => seriesActivity?.SetTag("library.import.series.outcome", "persisted"))
            // Per-item scope context — additive to error.LogAction()'s existing log (which already
            // carries Container, PartitionKey, Id). MalId is omitted since the error's Id is the MAL ID.
            .AddLogOnFailure(_ => logger => logger.LogWarning("Failed to persist series {Id}-{Title} of {SeriesType}", withCover.Id, withCover.Titles.Default, withCover.GetType().Name))
            .AddLogOnFailure(error => error.LogAction());
    }

    // Parse/validation errors and non-transient Cosmos errors are permanent: the item can't
    // succeed on retry, so we skip it rather than retry the whole page.
    private static bool IsPermanent(DomainError error) =>
        error is DomainValidationErrors or CosmosResponseError {IsTransient: false};

    private static ImportResult Map(SeriesSeason season, IEnumerable<CosmosOperationCost> charges)
    {
        // Every series on a page shares the page's season (stamped during mapping), so it's the
        // result season directly — no grouping, and safe when the page filtered down to nothing.
        var list = charges as IReadOnlyCollection<CosmosOperationCost> ?? charges.ToList();
        return new ImportResult(
            season,
            list.Count,
            list.Aggregate(default(CosmosOperationCost), (acc, charge) => acc + charge));
    }

    private static void SetPageActivityTags(Activity? activity, BulkResult<ImportResult> bulkResult)
    {
        if (activity is null) return;
        var succeeded = bulkResult.Value.Imported;
        var totalCost = bulkResult.Value.TotalCost.RuUsed;
        var failed = bulkResult is PartialSuccessBulkResult<ImportResult> partial ? partial.Errors.Length : 0;
        activity
            .SetTag("library.import.page.succeeded", succeeded)
            .SetTag("library.import.page.failed", failed)
            .SetTag("library.import.page.total_cost", Math.Round(totalCost, 2));
        if (succeeded > 0)
            activity.SetTag("library.import.page.avg_cost", Math.Round(totalCost / succeeded, 2));
    }

    private static void SetImportActivityTags(Activity? activity, ImportResult result)
    {
        if (activity is null) return;
        var totalCost = result.TotalCost.RuUsed;
        activity
            .SetTag("library.import.total_imported", result.Imported)
            .SetTag("library.import.total_cost", Math.Round(totalCost, 2));
        if (result.Imported > 0)
            activity.SetTag("library.import.avg_cost", Math.Round(totalCost / result.Imported, 2));
    }


    private record ImportResult(SeriesSeason SeriesSeason, int Imported, CosmosOperationCost TotalCost);
}