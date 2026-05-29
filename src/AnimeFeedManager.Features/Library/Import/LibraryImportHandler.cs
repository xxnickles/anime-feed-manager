using System.Diagnostics;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Mappers;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;

namespace AnimeFeedManager.Features.Library.Import;

internal sealed class LibraryImportHandler(
    IJikanClient jikan,
    SingleSeriesPersistenceHandler<double> persistSeries,
    LibrarySeasonsIndexUpserter upsertIndex,
    TimeProvider time,
    ILogger<LibraryImportHandler> logger)
    : WorkHandler<LibraryImportCommand>
{
    private static readonly ActivitySource Source = new(Telemetry.LibraryImportSource);

    public override int Capacity => 5;
    public override BoundedChannelFullMode FullMode => BoundedChannelFullMode.DropOldest;

    public override async Task<Result<Unit>> Handle(
        LibraryImportCommand command,
        CancellationToken cancellationToken)
    {
        using var importActivity = Source.StartActivity("Library.Import");
        importActivity?.SetTag("library.import.target", command.Target.GetType().Name);

        var source = command.Target switch
        {
            ImportTarget.CurrentSeason => jikan.GetCurrentSeason(cancellationToken),
            ImportTarget.SpecificSeason s => jikan.GetSeason(s.SeriesSeason.Year, s.SeriesSeason.Season,
                cancellationToken),
            _ => throw new UnreachableException()
        };

        var now = time.GetUtcNow();
        return await PersistSeries(source, now, persistSeries, cancellationToken)
            .Tap(result => SetImportActivityTags(importActivity, result))
            .AddLogOnSuccess(LogFactories.Log<ImportResult>((result, iLogger) =>
                iLogger.LogInformation("Imported {Imported} series", result.Imported)))
            .Bind(result => upsertIndex(new SeasonEntry(result.SeriesSeason, now, result.Imported), cancellationToken))
            .FlushLogs(logger)
            .Map(_ => new Unit())
            .MarkActivityErroredOnError();
    }

    private static async Task<Result<ImportResult>> PersistSeries(
        IAsyncEnumerable<Result<JikanPage>> source,
        DateTimeOffset now,
        SingleSeriesPersistenceHandler<double> seriesPersistenceHandler,
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
                    .Bind(page => ProcessPage(page, seriesPersistenceHandler, now, cancellationToken))
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
        SingleSeriesPersistenceHandler<double> persistSeries,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tasks = page.Items.Select(item =>
            item.ToSeries(now)
                .Bind(parsed => persistSeries(parsed, cancellationToken)
                    // Keeps the log trace
                    .AddLogOnFailure(error => error.LogAction())
                    // We want to retry on transient errors, for that we make permanent errors a good result
                    .BindOnErrorWhen(
                        binder: _ => Result<double>.Success(0.0),
                        predicate: err => err is CosmosResponseError {IsTransient: false})
                    .Map(charge => (parsed.SeriesSeason, charge)))
        );
        var results = await Task.WhenAll(tasks);
        return results.Flatten(Map);
    }

    private static ImportResult Map(IEnumerable<(SeriesSeason season, double charge)> items)
    {
        var list = items.ToList();
        var grouped = list.GroupBy(x => x.season)
            .OrderByDescending(g => g.Count())
            .ToList();
        return new ImportResult(
            grouped[0].Key,
            list.Count,
            list.Sum(x => x.charge));
    }

    private static void SetPageActivityTags(Activity? activity, BulkResult<ImportResult> bulkResult)
    {
        if (activity is null) return;
        var succeeded = bulkResult.Value.Imported;
        var totalCost = bulkResult.Value.TotalCost;
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
        activity
            .SetTag("library.import.total_imported", result.Imported)
            .SetTag("library.import.total_cost", Math.Round(result.TotalCost, 2));
        if (result.Imported > 0)
            activity.SetTag("library.import.avg_cost", Math.Round(result.TotalCost / result.Imported, 2));
    }


    private record ImportResult(SeriesSeason SeriesSeason, int Imported, double TotalCost);
}