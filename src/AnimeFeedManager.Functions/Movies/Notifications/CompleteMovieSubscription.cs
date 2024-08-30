using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Notifications;

public class CompleteMovieSubscription
{
    private readonly IMovieSubscriptionStore _movieSubscriptionStore;
    private readonly ILogger<CompleteMovieSubscription> _logger;

    public CompleteMovieSubscription(
        IMovieSubscriptionStore movieSubscriptionStore,
        ILogger<CompleteMovieSubscription> logger)
    {
        _movieSubscriptionStore = movieSubscriptionStore;
        _logger = logger;
    }

    [Function(nameof(CompleteMovieSubscription))]
    public async Task Run(
        [QueueTrigger(CompleteMovieSubscriptionEvent.TargetQueue, Connection = Constants.AzureConnectionName)]
        CompleteMovieSubscriptionEvent message, CancellationToken token)
    {
        var entity = message.MoviesInformation;
        entity.Processed = true;
        var result = await _movieSubscriptionStore.Upsert(entity,token);
        result.Match(
            _ => _logger.LogInformation("Notification for Movie subscription {Movie} for {Subscriber} has been completed", entity.RowKey, entity.PartitionKey),
            error => error.LogError(_logger)
        );
    }
}

