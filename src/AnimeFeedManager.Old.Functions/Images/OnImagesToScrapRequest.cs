using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Images;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Images;

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

    [Function(nameof(OnImagesToScrapRequest))]
    public async Task Run(
        [QueueTrigger(ScrapImagesRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapImagesRequest notification, CancellationToken token)
    {
        _logger.LogInformation("Images scrapping process for {Count} will be enqueue", notification.Events.Count);
        await _scrapImagesNotificationHandler.Handle(notification, token);
    }
}