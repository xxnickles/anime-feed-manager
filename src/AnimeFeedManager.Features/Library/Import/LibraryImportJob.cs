using AnimeFeedManager.Features.Library.Images;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Azure.Storage.Blobs;

namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Composition root for the library import. Builds the persistence, seasons-index, and
/// cover-image delegates from injected infrastructure once, hands them to the static
/// <see cref="LibraryImport"/> for any <see cref="ImportTarget"/>, and flushes the accumulated
/// logs. The single seam shared by every trigger source: the weekly
/// <see cref="LibraryImportCronJob"/> (<c>ImportTarget.Now()</c>) and the manual admin triggers
/// (latest or a specific season). Run as fire-and-forget background work via <c>JobExecutor</c>
/// under the <c>"library-import"</c> single-flight gate, so only one import runs at a time
/// regardless of trigger source.
/// </summary>
public sealed class LibraryImportJob(
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    IImageHttpClient imageHttpClient,
    BlobServiceClient blobServiceClient,
    TimeProvider time,
    ILogger<LibraryImportJob> logger)
{
    private readonly SingleSeriesPersistenceHandler<CosmosOperationCost> _persistSeries =
        cosmosFactory.CosmosSingleSeriesPersistenceHandler();

    private readonly LibrarySeasonsIndexUpserter _upsertIndex =
        cosmosFactory.LibrarySeasonsIndexUpserterHandler();

    private readonly SeriesImageProcessor _processImage =
        new ImageProcessorDependencies(imageHttpClient, blobServiceClient).SeriesImageProcessorHandler();

    public Task Run(ImportTarget target, CancellationToken cancellationToken) =>
        LibraryImport
            .Execute(target, jikan, _persistSeries, _upsertIndex, _processImage, time, cancellationToken)
            .FlushLogs(logger);
}
