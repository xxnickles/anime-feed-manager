using System.Net.Http;
using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UploadImage
{
    private readonly IMediator _mediator;
    private readonly IImagesStore _imagesStore;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadImage> _logger;

    public UploadImage(IMediator mediator, IImagesStore imagesStore, HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _imagesStore = imagesStore;
        _logger = loggerFactory.CreateLogger<UploadImage>();
        _httpClient = httpClient;

        _httpClient.DefaultRequestHeaders.Add("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
    }


    [Function("UploadImage")]
    public async Task Run(
        [QueueTrigger(QueueNames.ImageProcess, Connection = "AzureWebJobsStorage")]
        BlobImageInfoEvent imageInfoEvent
        )
    {
        _logger.LogInformation("Getting image for {Name} from {RemoteUrl}", imageInfoEvent.BlobName,
            imageInfoEvent.RemoteUrl);
        var response = await _httpClient.GetAsync(imageInfoEvent.RemoteUrl);
        response.EnsureSuccessStatusCode();
        await using var ms = await response.Content.ReadAsStreamAsync(default);

        var fileLocation = await _imagesStore.Upload($"{imageInfoEvent.BlobName}.jpg", imageInfoEvent.Directory, ms);
        _logger.LogInformation("{BlobName} has been uploaded", imageInfoEvent.BlobName);

        // Update AnimeInfo
        var imageStorage = new ImageStorage
        {
            ImageUrl = fileLocation.AbsoluteUri,
            PartitionKey = imageInfoEvent.Partition,
            RowKey = imageInfoEvent.Id
        }.AddEtag();

        await UpdateAnimeInfo(imageStorage);
    }

    private async Task UpdateAnimeInfo(ImageStorage imageStorage)
    {
        var result = await _mediator.Send(new AddImageUrlCmd(imageStorage));
        result.Match(
            _ => _logger.LogInformation("{ImageStorageRowKey} has been updated", imageStorage.RowKey),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}