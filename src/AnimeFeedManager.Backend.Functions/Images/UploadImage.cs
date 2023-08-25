using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Images;

public sealed class UploadImage
{
    private readonly ImageAdder _imageAdder;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadImage> _logger;

    public UploadImage(
        ImageAdder imageAdder,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _imageAdder = imageAdder;
        _logger = loggerFactory.CreateLogger<UploadImage>();
        _httpClient = httpClientFactory.CreateClient();

        _httpClient.DefaultRequestHeaders.Add("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
    }


    [Function("UploadImage")]
    public async Task Run(
        [QueueTrigger(Box.Available.ImageProcessBox, Connection = "AzureWebJobsStorage")]
        StateWrap<DownloadImageEvent> imageScrapEvent
    )
    {
        _logger.LogInformation("Getting image for {Name} from {RemoteUrl}", imageScrapEvent.Payload.BlobName,
            imageScrapEvent.Payload.RemoteUrl);

        
            var response = await _httpClient.GetAsync(imageScrapEvent.Payload.RemoteUrl);
            response.EnsureSuccessStatusCode();
            await using var ms = await response.Content.ReadAsStreamAsync(default);

            var result = await _imageAdder.Add(ms, imageScrapEvent);

            result.Match(
                _ => _logger.LogInformation(""),
                e => e.LogDomainError(_logger));
    }
}