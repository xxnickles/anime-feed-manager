using Azure.Core;

namespace AnimeFeedManager.Features.Infrastructure;

public abstract record AzureStorageSettings;

public sealed record ConnectionStringSettings(string StorageConnectionString) : AzureStorageSettings;

public sealed record TokenCredentialSettings(QueueUri QueueUri, BlobUri BlobUri, Func<TokenCredential> DefaultTokenCredential) : AzureStorageSettings;

public readonly record struct QueueUri(Uri Uri)
{
    public static implicit operator Uri(QueueUri queueUri) => queueUri.Uri;
}

public readonly record struct BlobUri(Uri Uri)
{
    public static implicit operator Uri(BlobUri blobUri) => blobUri.Uri;
}