using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Seasons.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class OnLatestSeasonsUpdate
{
    private readonly ILatestSeasonStore _latestSeasonStore;
    private readonly ILogger<OnLatestSeasonsUpdate> _logger;
    
    public OnLatestSeasonsUpdate(ILatestSeasonStore latestSeasonStore, ILoggerFactory loggerFactory)
    {
        _latestSeasonStore = latestSeasonStore;
        _logger = loggerFactory.CreateLogger<OnLatestSeasonsUpdate>();
    }

    [Function("OnLatestSeasonsUpdate")]
    public async Task Run(
        [QueueTrigger(UpdateLatestSeasonsRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        UpdateLatestSeasonsRequest notification)
    {
        _logger.LogInformation("Updating latest seasons information");
        var result = await _latestSeasonStore.Update(default);
        result.Match(
            _ => _logger.LogInformation("Latest Season have been stored"),
            e => e.LogError(_logger)
        );
    }
}