using AnimeFeedManager.Features.Infrastructure.Messaging;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnimeFeedManager.Features.Images.IO;

public class AzureImagesBlobStore : IImagesBlobStore
{
    private const string Container = "images";
    private BlobContainerClient _containerClient;

    public AzureImagesBlobStore(AzureStorageSettings storageOptions)
    {
        _containerClient = GetClient(storageOptions);
        Initialize();
    }

    private static BlobContainerClient GetClient(AzureStorageSettings azureSettings)
    {
        return azureSettings switch
        {
            ConnectionStringSettings connectionStringOptions => new BlobContainerClient(
                connectionStringOptions.StorageConnectionString, Container),
            TokenCredentialSettings tokenCredentialOptions => new BlobContainerClient(
                new Uri(tokenCredentialOptions.BlobUri, Container), tokenCredentialOptions.DefaultTokenCredential()),
            _ => throw new ArgumentException(
                "Provided Table Storage configuration is not valid. Make sure Configurations for Azure table Storage is correct for either connection string or managed identities",
                nameof(TableClientOptions))
        };
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
}