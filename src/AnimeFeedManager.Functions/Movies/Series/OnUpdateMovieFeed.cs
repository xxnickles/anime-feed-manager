using AnimeFeedManager.Features.Movies.Scrapping.Feed;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Series;

public class OnUpdateMovieFeed
{
    private readonly MovieFeedUpdateStore _feedUpdateStore;
    private readonly ILogger<OnUpdateMovieFeed> _logger;

    public OnUpdateMovieFeed(
        MovieFeedUpdateStore feedUpdateStore,
        ILogger<OnUpdateMovieFeed> logger)
    {
        _feedUpdateStore = feedUpdateStore;
        _logger = logger;
    }

    [Function(nameof(OnUpdateMovieFeed))]
    public async Task Run(
        [QueueTrigger(UpdateMovieFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        UpdateMovieFeed message, CancellationToken token)
    {
        var results = await _feedUpdateStore.StoreFeedUpdates(message.Series, message.Links, token);

        results.Match(
            result => LogMessage(result, message.Series),
            error => error.LogError(_logger));
    }

    private void LogMessage(MovieFeedScrapResult result, MovieStorage storage)
    {
        switch (result)
        {
            case MovieFeedScrapResult.NotFound:
                _logger.LogWarning(
                    "{Series} for season {Season} feed scrap has been completed, but not matches were found",
                    storage.Title, storage.PartitionKey);
                break;
            case MovieFeedScrapResult.FoundAndUpdated:
                _logger.LogInformation(
                    "{Series} for season {Season} feed scrap has been completed, matches have been found and storage has been updated",
                    storage.Title, storage.PartitionKey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}