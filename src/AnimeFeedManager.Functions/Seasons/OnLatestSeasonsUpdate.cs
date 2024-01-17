using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class OnLatestSeasonsUpdate
{
    private readonly ISortedSeasons _sortedSeasons;
    private readonly ILogger<OnLatestSeasonsUpdate> _logger;
    
    public OnLatestSeasonsUpdate(ISortedSeasons sortedSeasons, ILoggerFactory loggerFactory)
    {
        _sortedSeasons = sortedSeasons;
        _logger = loggerFactory.CreateLogger<OnLatestSeasonsUpdate>();
    }

    [Function("OnLatestSeasonsUpdate")]
    public async Task Run(
        [QueueTrigger(Box.Available.LatestSeasonsBox, Connection = "AzureWebJobsStorage")]
        UpdateLatestSeasonsRequest notification)
    {
        var result = await _sortedSeasons.Update(default);
        result.Match(
            _ => _logger.LogInformation("Latest Season have been stored"),
            e => e.LogError(_logger)
        );
    }
}