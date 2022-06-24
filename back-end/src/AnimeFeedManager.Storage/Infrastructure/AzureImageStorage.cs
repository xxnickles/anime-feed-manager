using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Storage.Infrastructure;


public sealed class AzureBlobStorageOptions
{
    public string? StorageConnectionString { get; set; }
}

public interface IImagesStore
{
    public Task<Uri> Upload(string fileName, string path, Stream data);
}

public class AzureStorageBlobStore : IImagesStore
{
    private readonly BlobContainerClient _containerClient;
    private const string Container = "images";

    public AzureStorageBlobStore(IOptionsSnapshot<AzureBlobStorageOptions> blobStorageOptions)
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