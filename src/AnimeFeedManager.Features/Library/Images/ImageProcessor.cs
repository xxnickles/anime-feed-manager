using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// The two resources the image processor needs, bundled so the processor can stay a static
/// function over them instead of a stateful injected class. The handler builds this from its
/// injected dependencies and constructs the <see cref="SeriesImageProcessor"/> delegate locally.
/// </summary>
public sealed record ImageProcessorDependencies(IImageHttpClient HttpClient, BlobServiceClient BlobServiceClient);

/// <summary>
/// Downloads the cover named by a <see cref="ProcessSeriesImageCommand"/> and stores it in blob
/// storage, yielding the relative blob path to persist as the series' <c>CoverImageUrl</c>. Built
/// from <see cref="ImageProcessorDependencies"/> via <see cref="ImageProcessor.SeriesImageProcessorHandler"/>.
/// </summary>
public delegate Task<Result<string>> SeriesImageProcessor(
    ProcessSeriesImageCommand command,
    CancellationToken cancellationToken);


/// <summary>
/// Downloads a series cover image and stores it in blob storage, returning the relative blob
/// path to persist as the series' <c>CoverImageUrl</c>. Idempotent: if the target blob already
/// exists (frequent on re-import) the download is skipped and the existing path returned, so a
/// previously-patched cover is never re-fetched or clobbered. The <c>images</c> container is
/// provisioned once at startup, so this never creates it.
/// </summary>
public static class ImageProcessor
{
    public const string BlobContainer = "images";

    public static SeriesImageProcessor SeriesImageProcessorHandler(this ImageProcessorDependencies deps) =>
        (command, cancellationToken) => Process(deps, command, cancellationToken);

    private static async Task<Result<string>> Process(
        ImageProcessorDependencies deps,
        ProcessSeriesImageCommand command,
        CancellationToken cancellationToken)
    {
        // Jikan serves WebP (preferred by PickCover) or JPG. Derive the extension and
        // content-type from the source URL so the stored blob is served with the right type.
        var isWebp = command.SourceUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        // Blob layout images/{year}/{season}/{malId}.{ext} — covers nest under year then season
        // so no single path segment accumulates an unbounded number of blobs.
        var blobName = $"{command.Season.Year}/{command.Season.Season}/{command.Id}.{(isWebp ? "webp" : "jpg")}";
        var relativePath = $"{BlobContainer}/{blobName}";

        try
        {
            var blob = deps.BlobServiceClient.GetBlobContainerClient(BlobContainer).GetBlobClient(blobName);

            // Re-imports are frequent; never re-download or clobber an already-stored cover.
            if (await blob.ExistsAsync(cancellationToken))
                return relativePath;

            var bytes = await deps.HttpClient.DownloadImage(command.SourceUrl, cancellationToken);

            var headers = new BlobHttpHeaders { ContentType = isWebp ? "image/webp" : "image/jpeg" };
            await blob.UploadAsync(BinaryData.FromBytes(bytes), new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

            return relativePath;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
