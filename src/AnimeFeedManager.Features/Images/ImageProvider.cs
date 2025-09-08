using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnimeFeedManager.Features.Images;

public sealed record ImageProcessData(
    string FileName,
    string TargetDirectory,
    Uri Url);

public interface IImageProvider
{
    Task<Result<Uri>> Process(ImageProcessData data, CancellationToken cancellationToken = default);
}

public class ImageProvider : IImageProvider
{
    public const string Container = "images";

    private readonly HttpClient _httpClient;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<ImageProvider> _logger;

    public ImageProvider(
        HttpClient httpClient,
        BlobServiceClient blobServiceClient,
        ILogger<ImageProvider> logger)
    {
        _httpClient = httpClient;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<Result<Uri>> Process(
        ImageProcessData data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Downloading image for {Name} from {RemoteUrl}", data.FileName, data.Url);
            var response = await _httpClient.GetAsync(data.Url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await Upload(_blobServiceClient, data.FileName, data.TargetDirectory, stream, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when processing image {ImageName}", data.FileName);
            return new HandledError();
        }
    }

    private static async Task<Uri> Upload(
        BlobServiceClient blobServiceClient,
        string fileName,
        string path,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(Container);
        // Normalize path and file name to use forward slashes and .jpg extension
        var blobName = $"{path.Trim('/').Replace('\\', '/')}/{fileName}.jpg";
        var blob = container.GetBlobClient(blobName);
        var blobHttpHeader = new BlobHttpHeaders {ContentType = "image/jpeg"};
        await blob.UploadAsync(data, new BlobUploadOptions {HttpHeaders = blobHttpHeader}, cancellationToken);
        return new Uri($"{Container}/{blobName}", UriKind.Relative);
    }
}