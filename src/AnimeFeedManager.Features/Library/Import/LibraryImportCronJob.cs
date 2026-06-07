using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Azure.Storage.Blobs;

namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Weekly library import: every Saturday at 04:00 UTC, runs an import for the current season.
/// Acts as the composition root for the import — builds the persistence, seasons-index, and
/// cover-image delegates from injected infrastructure, hands them to the static
/// <see cref="LibraryImport"/>, and flushes the accumulated logs. The cron expression is
/// overridable via configuration (<see cref="CronJobOverride"/>); SkipIfRunning is inherited
/// <c>true</c> so a long-running prior fire is skipped rather than overlapping (the executor's
/// per-job gate).
/// </summary>
internal sealed class LibraryImportCronJob(
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    IImageHttpClient imageHttpClient,
    BlobServiceClient blobServiceClient,
    TimeProvider time,
    ILogger<LibraryImportCronJob> logger) : CronJob
{
    private readonly SingleSeriesPersistenceHandler<CosmosOperationCost> _persistSeries =
        cosmosFactory.CosmosSingleSeriesPersistenceHandler();

    private readonly LibrarySeasonsIndexUpserter _upsertIndex =
        cosmosFactory.LibrarySeasonsIndexUpserterHandler();

    private readonly SeriesImageProcessor _processImage =
        new ImageProcessorDependencies(imageHttpClient, blobServiceClient).SeriesImageProcessorHandler();

    public override string Name => "library-import";

    public override string DefaultExpression => "0 4 * * 6";

    public override Task Run(CancellationToken cancellationToken) =>
        LibraryImport
            .Execute(ImportTarget.Now(), jikan, _persistSeries, _upsertIndex, _processImage, time, cancellationToken)
            .FlushLogs(logger);
}
