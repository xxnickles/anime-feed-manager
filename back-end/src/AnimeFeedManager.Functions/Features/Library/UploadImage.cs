using System.Net.Http;
using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UploadImage
{
    private readonly IMediator _mediator;
    private readonly IImagesStore _imagesStore;
    private readonly HttpClient _httpClient;

    public UploadImage(IMediator mediator, IImagesStore imagesStore, HttpClient httpClient)
    {
        _mediator = mediator;
        _imagesStore = imagesStore;
        _httpClient = httpClient;

        _httpClient.DefaultRequestHeaders.Add("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
    }


    [FunctionName("UploadImage")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [QueueTrigger(QueueNames.ImageProcess, Connection = "AzureWebJobsStorage")]
        BlobImageInfoEvent imageInfoEvent,
        ILogger log)
    {
        log.LogInformation("Getting image for {Name} from {RemoteUrl}", imageInfoEvent.BlobName,
            imageInfoEvent.RemoteUrl);
        var response = await _httpClient.GetAsync(imageInfoEvent.RemoteUrl);
        response.EnsureSuccessStatusCode();
        await using var ms = await response.Content.ReadAsStreamAsync(default);

        var fileLocation = await _imagesStore.Upload($"{imageInfoEvent.BlobName}.jpg", imageInfoEvent.Directory, ms);
        log.LogInformation("{BlobName} has been uploaded", imageInfoEvent.BlobName);

        // Update AnimeInfo
        var imageStorage = new ImageStorage
        {
            ImageUrl = fileLocation.AbsoluteUri,
            PartitionKey = imageInfoEvent.Partition,
            RowKey = imageInfoEvent.Id
        }.AddEtag();

        await UpdateAnimeInfo(imageStorage, log);
    }

    private async Task UpdateAnimeInfo(ImageStorage imageStorage, ILogger log)
    {
        var result = await _mediator.Send(new AddImageUrl(imageStorage));
        result.Match(
            _ => log.LogInformation("{ImageStorageRowKey} has been updated", imageStorage.RowKey),
            e => log.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}