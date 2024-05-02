using AnimeFeedManager.Features.Ovas.Scrapping.Feed;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnUpdateOvaFeed
{
    private readonly OvaFeedUpdateStore _feedUpdateStore;
    private readonly ILogger<OnUpdateOvaFeed> _logger;

    public OnUpdateOvaFeed(
        OvaFeedUpdateStore feedUpdateStore,
        ILogger<OnUpdateOvaFeed> logger)
    {
        _feedUpdateStore = feedUpdateStore;
        _logger = logger;
    }

    [Function(nameof(OnUpdateOvaFeed))]
    public async Task Run(
        [QueueTrigger(UpdateOvaFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        UpdateOvaFeed message, CancellationToken token)
    {
        var results = await _feedUpdateStore.StoreFeedUpdates(message.Series, message.Links, token);

        results.Match(
            result => LogMessage(result, message.Series),
            error => error.LogError(_logger));
    }

    private void LogMessage(OvaFeedScrapResult result, OvaStorage storage)
    {
        switch (result)
        {
            case OvaFeedScrapResult.NotFound:
                _logger.LogWarning(
                    "{Series} for season {Season} feed scrap has been completed, but not matches were found",
                    storage.Title, storage.PartitionKey);
                break;
            case OvaFeedScrapResult.FoundAndUpdated:
                _logger.LogInformation(
                    "{Series} for season {Season} feed scrap has been completed, matches have been found and storage has been updated",
                    storage.Title, storage.PartitionKey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}