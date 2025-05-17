using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnimeFeedManager.Features.Images;

public sealed record DownloadedImage(string FileName, Stream Data); 

public interface IImagesStore
{
    Task<Result<DownloadedImage>> Download(string fileName, Uri url, CancellationToken cancellationToken = default);
    Task<Result<Uri>> Upload(string fileName, string path, Stream data, CancellationToken cancellationToken = default);
}

public class ImagesStore : IImagesStore
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImagesStore> _logger;
    private const string Container = "images";
    private readonly BlobContainerClient _containerClient;

    public ImagesStore(AzureStorageSettings storageOptions,
        IHttpClientFactory httpClientFactory,
        ILogger<ImagesStore> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _logger = logger;
        _containerClient = GetClient(storageOptions);
        Initialize();
    }


    public async Task<Result<DownloadedImage>> Download(string fileName, Uri url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Downloading image for {Name} from {RemoteUrl}", fileName, url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return Result<DownloadedImage>.Success(new DownloadedImage(fileName, stream));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when downloading image {ImageName}", fileName);
            return Result<DownloadedImage>.Failure(new HandledError());
        }
        
    }

    public async Task<Result<Uri>> Upload(string fileName, string path, Stream data, CancellationToken cancellationToken = default)
    {
        try
        {
            var finalPath = Path.Combine(path, fileName);
            var blob = _containerClient.GetBlobClient(finalPath);
            var blobHttpHeader = new BlobHttpHeaders {ContentType = "image/jpg"};
            await blob.UploadAsync(data, new BlobUploadOptions {HttpHeaders = blobHttpHeader}, cancellationToken);
            return Result<Uri>.Success(blob.Uri); 
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when uploading image {ImageName}", fileName);
            return Result<Uri>.Failure(new HandledError());
        }
      
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
}