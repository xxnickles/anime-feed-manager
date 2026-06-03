using AnimeFeedManager.Features.Library.Images.Storage;
using Azure.Storage.Blobs;

namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Work-queue handler for <see cref="ProcessSeriesImageCommand"/>: downloads the cover into blob
/// storage then patches the series' <c>CoverImageUrl</c> to the stored path. Both steps are built
/// as delegate fields (the functional core) from the injected abstractions — the processor never
/// touches the Cosmos factory and the patcher never touches blob storage.
/// </summary>
internal sealed class ProcessSeriesImageHandler(
    ICosmosContainerFactory cosmosFactory,
    BlobServiceClient blobServiceClient,
    IImageHttpClient httpClient,
    ILogger<ProcessSeriesImageHandler> logger)
    : WorkHandler<ProcessSeriesImageCommand>
{
    private readonly SeriesImageProcessor _processImage =
        new ImageProcessorDependencies(httpClient, blobServiceClient).SeriesImageProcessorHandler();

    private readonly CoverImageUrlPatcher _patchCoverImageUrl =
        cosmosFactory.CoverImageUrlPatcherHandler();

    // Image work is never stale, so back-pressure (Wait) rather than DropOldest; a large buffer
    // absorbs a page's worth of enqueues without blocking the import. Tune after observing throughput.
    public override int Capacity => 200;
    public override BoundedChannelFullMode FullMode => BoundedChannelFullMode.Wait;

    public override Task<Result<Unit>> Handle(ProcessSeriesImageCommand command, CancellationToken cancellationToken) =>
        _processImage(command, cancellationToken)
            .Bind(blobPath => _patchCoverImageUrl(command.Id, command.Season, blobPath, cancellationToken))
            .AddLogOnFailure(error => error.LogAction())
            .FlushLogs(logger);
}
