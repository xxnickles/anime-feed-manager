using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Provisions the <see cref="ImageProcessor.BlobContainer"/> container once at startup so the
/// image processor never has to create it on the hot path. Implemented as a one-shot
/// <see cref="IHostedService"/> rather than a <see cref="BackgroundService"/>: the work belongs in
/// <see cref="StartAsync"/> so host startup blocks until the container exists, guaranteeing the
/// import (and its enqueued image work) can never run against a missing container.
/// Covers are served directly from blob storage, so the container is created with
/// <see cref="PublicAccessType.Blob"/> (anonymous read of individual blobs, no listing). The
/// access level is also re-asserted on every start so a pre-existing private container (e.g. a
/// persisted Azurite volume created before this change) is flipped public. Production additionally
/// requires the storage account's <c>AllowBlobPublicAccess</c> to be enabled.
/// </summary>
internal sealed class ImagesContainerInitializer(
    BlobServiceClient blobServiceClient,
    ILogger<ImagesContainerInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var container = blobServiceClient.GetBlobContainerClient(ImageProcessor.BlobContainer);

        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        await container.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        logger.LogInformation(
            "Blob container '{Container}' is provisioned (public blob access) and ready.",
            ImageProcessor.BlobContainer);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
