using System.Diagnostics;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Mappers;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;

namespace AnimeFeedManager.Features.Library.Import;

internal sealed class LibraryImportHandler(
    IJikanClient jikan,
    SingleSeriesPersistenceHandler<double> persistSeries,
    LibrarySeasonsIndexUpserter upsertIndex,
    TimeProvider time,
    ILogger<LibraryImportHandler> logger)
    : WorkHandler<LibraryImportCommand>
{
    public override int Capacity => 5;
    public override BoundedChannelFullMode FullMode => BoundedChannelFullMode.DropOldest;

    public override async Task<Result<Unit>> Handle(
        LibraryImportCommand command,
        CancellationToken cancellationToken)
    {
        var source = command.Target switch
        {
            ImportTarget.CurrentSeason => jikan.GetCurrentSeason(cancellationToken),
            ImportTarget.SpecificSeason s => jikan.GetSeason(s.SeriesSeason.Year, s.SeriesSeason.Season,
                cancellationToken),
            _ => throw new UnreachableException()
        };

        var now = time.GetUtcNow();
        return await PersistSeries(source, now, persistSeries, cancellationToken)
            .AddLogOnSuccess(LogFactories.Log<ImportResult>((result, iLogger) =>
                iLogger.LogInformation("Imported {Imported} series", result.Imported)))
            .Bind(result => upsertIndex(new SeasonEntry(result.SeriesSeason, now, result.Imported), cancellationToken))
            .FlushLogs(logger)
            .Map(_ => new Unit());
    }

    private static async Task<Result<ImportResult>> PersistSeries(
        IAsyncEnumerable<Result<JikanPage>> source,
        DateTimeOffset now,
        SingleSeriesPersistenceHandler<double> seriesPersistenceHandler,
        CancellationToken cancellationToken
    )
    {
        var accumulated = new List<Result<ImportResult>>();

        await foreach (var pageResult in source.WithCancellation(cancellationToken))
        {
            var persistResult =
                await pageResult.Bind(page => ProcessPage(page, seriesPersistenceHandler, now, cancellationToken))
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
                    });

            accumulated.AddRange(persistResult);
            // If there was full error, stop processing
            if (persistResult.IsFailure)
                break;
        }

        return accumulated
            .Flatten(results => results.Aggregate((first, next) =>
                first with {Imported = first.Imported + next.Imported}))
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
                    .Map(_ => parsed.SeriesSeason))
        );
        var results = await Task.WhenAll(tasks);
        return results.Flatten(Map);
    }

    private static ImportResult Map(IEnumerable<SeriesSeason> seasons)
    {
        var grouped = seasons.GroupBy(season => season)
            .OrderByDescending(g => g.Count())
            .ToList();
        return new ImportResult(
            grouped[0].Key,
            grouped.Sum(g => g.Count()));
    }


    private record ImportResult(SeriesSeason SeriesSeason, int Imported);
}