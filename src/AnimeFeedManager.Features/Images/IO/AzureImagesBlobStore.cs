using AnimeFeedManager.Features.Infrastructure.Messaging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Images.IO;

public class AzureImagesBlobStore : IImagesBlobStore, IDisposable
{
    private const string Container = "images";
    private BlobContainerClient _containerClient;
    private readonly IDisposable? _optionsReference;
    public AzureImagesBlobStore(IOptionsMonitor<AzureBlobStorageOptions> blobStorageOptions)
    {
        _containerClient = new BlobContainerClient(blobStorageOptions.CurrentValue.StorageConnectionString, Container);
        Initialize();

        _optionsReference = blobStorageOptions.OnChange(options =>
        {
            _containerClient = new BlobContainerClient(options.StorageConnectionString, Container);
            Initialize();
        });

    }

    private void Initialize()
    {
        _containerClient.CreateIfNotExists();
        var currentAccessPolicy = _containerClient.GetAccessPolicy();
        if (currentAccessPolicy == null || currentAccessPolicy.Value.BlobPublicAccess == PublicAccessType.None)
        {
            _containerClient.SetAccessPolicy(PublicAccessType.Blob);
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


    public void Dispose()
    {
        _optionsReference?.Dispose();
    }
}