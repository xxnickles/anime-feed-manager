using System.Diagnostics;
using AnimeFeedManager.Features.Library.Images.Storage;
using AnimeFeedManager.Shared;
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
    private static readonly ActivitySource Source = new(Telemetry.LibraryImageSource);

    private readonly SeriesImageProcessor _processImage =
        new ImageProcessorDependencies(httpClient, blobServiceClient).SeriesImageProcessorHandler();

    private readonly CoverImageUrlPatcher _patchCoverImageUrl =
        cosmosFactory.CoverImageUrlPatcherHandler();

    // Image work is never stale, so back-pressure (Wait) rather than DropOldest; a large buffer
    // absorbs a page's worth of enqueues without blocking the import. Tune after observing throughput.
    public override int Capacity => 200;
    public override BoundedChannelFullMode FullMode => BoundedChannelFullMode.Wait;

    public override async Task<Result<Unit>> Handle(ProcessSeriesImageCommand command, CancellationToken cancellationToken)
    {
        // Parented to the import's captured context so the download/upload/patch spans group under
        // the import trace. async method (not a plain expression body) so `using var activity`
        // stays current across the awaited chain — including MarkActivityErroredOnError at the end.
        using var activity = Source.StartActivity("Library.Image.Process", ActivityKind.Internal, command.ParentContext);
        activity?
            .SetTag("library.image.series_id", command.Id)
            .SetTag("library.image.season", command.Season.ToString())
            .SetTag("library.image.source_url", command.SourceUrl);

        return await _processImage(command, cancellationToken)
            .Tap(blobPath => activity?.SetTag("library.image.blob_path", blobPath))
            .AddLogOnSuccess(LogFactories.Log<string>((blobPath, log) =>
                log.LogInformation("Cover image stored for series {SeriesId} at {BlobPath}", command.Id, blobPath)))
            .Bind(blobPath => _patchCoverImageUrl(command.Id, command.Season, blobPath, cancellationToken)
                .AddLogOnSuccess(LogFactories.Log<Unit>((_, log) =>
                    log.LogInformation("Patched CoverImageUrl for series {SeriesId} to {BlobPath}", command.Id, blobPath))))
            .AddLogOnFailure(error => error.LogAction())
            .FlushLogs(logger)
            .MarkActivityErroredOnError();
    }
}
