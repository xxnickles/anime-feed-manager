using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Titles;

public sealed class OnUpdateSeasonTitles(
    SeasonTitlesUpdater updater,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnUpdateSeasonTitles> _logger = loggerFactory.CreateLogger<OnUpdateSeasonTitles>();

    [Function(nameof(OnUpdateSeasonTitles))]
    public async Task Run(
        [QueueTrigger(UpdateSeasonTitlesRequest.TargetQueue, Connection = Constants.AzureConnectionName)] UpdateSeasonTitlesRequest notification,
        CancellationToken token)
    {
        
        // Stores notification
        var result = await updater.Process(notification, token);
        result.Match(
            _ => _logger.LogInformation("Titles ({Count}) have been updated successfully",
                notification.Titles.Count.ToString()),
            e => e.LogError(_logger));
    }

}