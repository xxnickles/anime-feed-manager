using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Titles;

public sealed class OnUpdateSeasonTitles(
    SeasonTitlesUpdater updater,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnUpdateSeasonTitles> _logger = loggerFactory.CreateLogger<OnUpdateSeasonTitles>();

    [Function("OnUpdateSeasonTitles")]
    public async Task Run(
        [QueueTrigger(Box.Available.SeasonTitlesProcessBox, Connection = "AzureWebJobsStorage")] UpdateSeasonTitlesRequest notification)
    {
        
        // Stores notification
        var result = await updater.Process(notification, default);
        result.Match(
            _ => _logger.LogInformation("Titles ({Count}) have been updated successfully",
                notification.Titles.Count.ToString()),
            e => e.LogError(_logger));
    }

}