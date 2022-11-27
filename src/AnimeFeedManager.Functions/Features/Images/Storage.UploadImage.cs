using AnimeFeedManager.Application.MoviesLibrary.Commands;
using AnimeFeedManager.Application.OvasLibrary.Commands;
using AnimeFeedManager.Application.TvAnimeLibrary.Commands;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Images;

public class UploadImage
{
    private readonly IMediator _mediator;
    private readonly IImagesStore _imagesStore;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadImage> _logger;

    public UploadImage(IMediator mediator, IImagesStore imagesStore, IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _imagesStore = imagesStore;
        _logger = loggerFactory.CreateLogger<UploadImage>();
        _httpClient = httpClientFactory.CreateClient();

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

        try
        {
            var response = await _httpClient.GetAsync(imageInfoEvent.RemoteUrl);
            response.EnsureSuccessStatusCode();
            await using var ms = await response.Content.ReadAsStreamAsync(default);

            var fileLocation =
                await _imagesStore.Upload($"{imageInfoEvent.BlobName}.jpg", imageInfoEvent.Directory, ms);
            _logger.LogInformation("{BlobName} has been uploaded", imageInfoEvent.BlobName);

            // Update AnimeInfo
            var imageStorage = new ImageStorage
            {
                ImageUrl = fileLocation.AbsoluteUri,
                PartitionKey = imageInfoEvent.Partition,
                RowKey = imageInfoEvent.Id
            };

            await UpdateAnimeInfo(imageInfoEvent.SeriesType, imageStorage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred");
        }
    }

    private async Task UpdateAnimeInfo(SeriesType type, ImageStorage imageStorage)
    {
        var result = type switch
        {
            SeriesType.Tv => await _mediator.Send(new AddTvImageUrlCmd(imageStorage)),
            SeriesType.Ova => await _mediator.Send(new AddOvaImageUrlCmd(imageStorage)),
            SeriesType.Movie => await _mediator.Send(new AddMovieImageUrlCmd(imageStorage)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        result.Match(
            _ => _logger.LogInformation("{ImageStorageRowKey} ({Type}) has been updated", imageStorage.RowKey,
                type.ToString()),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}