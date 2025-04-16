using AnimeFeedManager.Features.Movies.Subscriptions;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Movies.Subscriptions;

public class OnMoviesFeedremoved
{
    private readonly MoviesSubscriptionStatusResetter _moviesSubscriptionStatusResetter;
    private readonly ILogger<OnMoviesFeedremoved> _logger;

    public OnMoviesFeedremoved(
        MoviesSubscriptionStatusResetter moviesSubscriptionStatusResetter,
        ILogger<OnMoviesFeedremoved> logger)
    {
        _moviesSubscriptionStatusResetter = moviesSubscriptionStatusResetter;
        _logger = logger;
    }

    [Function(nameof(OnMoviesFeedremoved))]
    public async Task Run(
        [QueueTrigger(MovieFeedRemovedEvent.TargetQueue, Connection = Constants.AzureConnectionName)]
        MovieFeedRemovedEvent notification, CancellationToken token)
    {
        var result = await RowKey.Parse(notification.Tile)
            .BindAsync(title => _moviesSubscriptionStatusResetter.ResetStatus(title, token));

        result.Match(
            _ => { _logger.LogInformation("Subscriptions for movie {Movie} have been reset", notification.Tile); },
            e => { e.LogError(_logger); }
        );
    }
}