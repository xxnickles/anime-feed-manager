using AnimeFeedManager.Features.Infrastructure.Messaging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Images.IO;

public class AzureImagesStore : IImagesStore
{
    private const string Container = "images";
    private readonly BlobContainerClient _containerClient;
    public AzureImagesStore(IOptionsSnapshot<AzureBlobStorageOptions> blobStorageOptions)
    {
        var blobStorageOptionsValue = blobStorageOptions.Value;
        _containerClient = new BlobContainerClient(blobStorageOptionsValue.StorageConnectionString, Container);
        _containerClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        var current = _containerClient.GetAccessPolicyAsync().GetAwaiter().GetResult().Value;
        if (current == null || current.BlobPublicAccess == PublicAccessType.None)
        {
            _containerClient.SetAccessPolicyAsync(PublicAccessType.Blob).GetAwaiter().GetResult();
        }
    }

    public async Task<Uri> Upload(string fileName, string path, Stream data)
    {
        var finalPath = Path.Combine(path, fileName);
        var blob = _containerClient.GetBlobClient(finalPath);
        var blobHttpHeader = new BlobHttpHeaders {ContentType = "image/jpg"};
        await blob.UploadAsync(data, new BlobUploadOptions {HttpHeaders = blobHttpHeader});
        return blob.Uri;
    }
}