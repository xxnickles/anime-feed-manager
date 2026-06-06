using Azure.Storage.Blobs;

namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Provisions the <see cref="ImageProcessor.BlobContainer"/> container once at startup so the
/// image processor never has to create it on the hot path. Implemented as a one-shot
/// <see cref="IHostedService"/> rather than a <see cref="BackgroundService"/>: the work belongs in
/// <see cref="StartAsync"/> so host startup blocks until the container exists, guaranteeing the
/// import (and its enqueued image work) can never run against a missing container.
/// Public blob access is deferred — the container is created private; covers are served via the app.
/// </summary>
internal sealed class ImagesContainerInitializer(
    BlobServiceClient blobServiceClient,
    ILogger<ImagesContainerInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await blobServiceClient
            .GetBlobContainerClient(ImageProcessor.BlobContainer)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        logger.LogInformation(
            "Blob container '{Container}' is provisioned and ready.", ImageProcessor.BlobContainer);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
