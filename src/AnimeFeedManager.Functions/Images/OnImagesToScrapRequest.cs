using AnimeFeedManager.Common.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Images;

public sealed class OnImagesToScrapRequest
{
    private readonly ScrapImagesNotificationHandler _scrapImagesNotificationHandler;

    public OnImagesToScrapRequest(ScrapImagesNotificationHandler scrapImagesNotificationHandler,
        ILoggerFactory loggerFactory)
    {
        _scrapImagesNotificationHandler = scrapImagesNotificationHandler;
        _logger = loggerFactory.CreateLogger<OnImagesToScrapRequest>();
    }

    private readonly ILogger<OnImagesToScrapRequest> _logger;

    [Function("OnImagesToScrapRequest")]
    public async Task Run(
        [QueueTrigger(ScrapImagesRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapImagesRequest notification)
    {
        _logger.LogInformation("Images scrapping process for {Count} will be enqueue", notification.Events.Count);
        await _scrapImagesNotificationHandler.Handle(notification, default);
    }
}