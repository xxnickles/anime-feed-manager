using System.Collections.Immutable;
using AnimeFeedManager.Old.Common;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Feed;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.Types;
using AnimeFeedManager.Old.Features.State.IO;
using AnimeFeedManager.Old.Features.State.Types;
using AnimeFeedManager.Old.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Movies.Series;

public class OnUpdateMovieFeed
{
    private readonly MovieFeedUpdateStore _feedUpdateStore;
    private readonly IStateUpdater _stateUpdater;
    private readonly IDomainPostman _domainPostman;
    private readonly IUserGetter _userGetter;
    private readonly ILogger<OnUpdateMovieFeed> _logger;

    public OnUpdateMovieFeed(
        MovieFeedUpdateStore feedUpdateStore,
        IStateUpdater stateUpdater,
        IDomainPostman domainPostman,
        IUserGetter userGetter,
        ILogger<OnUpdateMovieFeed> logger)
    {
        _feedUpdateStore = feedUpdateStore;
        _stateUpdater = stateUpdater;
        _domainPostman = domainPostman;
        _userGetter = userGetter;
        _logger = logger;
    }

    [Function(nameof(OnUpdateMovieFeed))]
    public async Task Run(
        [QueueTrigger(UpdateMovieFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        StateWrap<UpdateMovieFeed> message, CancellationToken token)
    {
        var results = await _feedUpdateStore.StoreFeedUpdates(message.Payload.Series, message.Payload.Links, token);

        results.Match(
            result => LogMessage(result, message.Payload.Series),
            error => error.LogError(_logger));

        var stateUpdate = await _stateUpdater.Update(results,
                new StateChange(message.StateId, NotificationTarget.Movie, message.Payload.Series.RowKey ?? string.Empty),
                token)
            .BindAsync(
                currentState =>
                    TryToPublishUpdate(currentState, message.Payload.Series.PartitionKey ?? string.Empty, token));


        stateUpdate.Match(
            _ => _logger.LogInformation("Movie feed notification has been sent"),
            error => error.LogError(_logger)
        );
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

    private async Task<Either<DomainError, Unit>> TryToPublishUpdate(CurrentState currentState, string seasonInfo,
        CancellationToken token)
    {
        if (!currentState.ShouldNotify) return unit;

        var notification = new MoviesFeedUpdateNotification(
            NotificationType.Information,
            SeriesType.Movie,
            seasonInfo,
            $"Movies feed for {seasonInfo} has been updated. Processed series: {currentState.Completed} Errors: {currentState.Errors}");

        return await _domainPostman.SendMessage(notification, token)
            .BindAsync(_ => SendMoviesFeedNotifications(seasonInfo, token));
    }
    
    private Task<Either<DomainError, Unit>> SendMoviesFeedNotifications(string seasonInfo, CancellationToken token)
    {
        return _userGetter.GetAvailableUsersData(token)
            .MapAsync(users => users.ConvertAll(user => new MoviesCheckFeedMatchesEvent(user.Email, user.UserId, seasonInfo)))
            .BindAsync(notification => SendNotifications(notification, token));
    }

    private async Task<Either<DomainError, Unit>> SendNotifications(ImmutableList<MoviesCheckFeedMatchesEvent> notifications,
        CancellationToken token)
    {
        var process = notifications.Select(notification => _domainPostman.SendMessage(notification, token)).ToArray();
        var results = await Task.WhenAll(process);
        return results.FlattenResults().Map(_ => unit);
    }
}