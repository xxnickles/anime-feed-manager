using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Seasons.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Seasons;

public sealed class OnLatestSeasonsUpdate
{
    private readonly ILatestSeasonStore _latestSeasonStore;
    private readonly ILogger<OnLatestSeasonsUpdate> _logger;
    
    public OnLatestSeasonsUpdate(ILatestSeasonStore latestSeasonStore, ILoggerFactory loggerFactory)
    {
        _latestSeasonStore = latestSeasonStore;
        _logger = loggerFactory.CreateLogger<OnLatestSeasonsUpdate>();
    }

    [Function(nameof(OnLatestSeasonsUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateLatestSeasonsRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        UpdateLatestSeasonsRequest notification, CancellationToken token)
    {
        _logger.LogInformation("Updating latest seasons information");
        var result = await _latestSeasonStore.Update(token);
        result.Match(
            _ => _logger.LogInformation("Latest Season have been stored"),
            e => e.LogError(_logger)
        );
    }
}