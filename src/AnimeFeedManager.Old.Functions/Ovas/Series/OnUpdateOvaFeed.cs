﻿using System.Collections.Immutable;
using AnimeFeedManager.Old.Common;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;
using AnimeFeedManager.Old.Features.State.IO;
using AnimeFeedManager.Old.Features.State.Types;
using AnimeFeedManager.Old.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Ovas.Series;

public class OnUpdateOvaFeed
{
    private readonly OvaFeedUpdateStore _feedUpdateStore;
    private readonly IStateUpdater _stateUpdater;
    private readonly IDomainPostman _domainPostman;
    private readonly IUserGetter _userGetter;
    private readonly ILogger<OnUpdateOvaFeed> _logger;

    public OnUpdateOvaFeed(
        OvaFeedUpdateStore feedUpdateStore,
        IStateUpdater stateUpdater,
        IDomainPostman domainPostman,
        IUserGetter userGetter,
        ILogger<OnUpdateOvaFeed> logger)
    {
        _feedUpdateStore = feedUpdateStore;
        _stateUpdater = stateUpdater;
        _domainPostman = domainPostman;
        _userGetter = userGetter;
        _logger = logger;
    }

    [Function(nameof(OnUpdateOvaFeed))]
    public async Task Run(
        [QueueTrigger(UpdateOvaFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        StateWrap<UpdateOvaFeed> message, CancellationToken token)
    {
        var results = await _feedUpdateStore.StoreFeedUpdates(message.Payload.Series, message.Payload.Links, token);

        results.Match(
            result => LogMessage(result, message.Payload.Series),
            error => error.LogError(_logger));

        var stateUpdate = await _stateUpdater.Update(results,
                new StateChange(message.StateId, NotificationTarget.Ova, message.Payload.Series.RowKey ?? string.Empty),
                token)
            .BindAsync(
                currentState =>
                    TryToPublishUpdate(currentState, message.Payload.Series.PartitionKey ?? string.Empty, token));

        stateUpdate.Match(
            _ => _logger.LogInformation("Ova feed notification has been sent"),
            error => error.LogError(_logger)
        );
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

    private async Task<Either<DomainError, Unit>> TryToPublishUpdate(CurrentState currentState, string seasonInfo,
        CancellationToken token)
    {
        if (!currentState.ShouldNotify) return unit;

        var notification = new OvasFeedUpdateNotification(
            NotificationType.Information,
            SeriesType.Ova,
            seasonInfo,
            $"Ovas feed for {seasonInfo} has been updated. Processed series: {currentState.Completed} Errors: {currentState.Errors}");

        return await _domainPostman.SendMessage(notification, token)
            .BindAsync(_ => SendOvasFeedNotifications(seasonInfo, token));
    }

    private Task<Either<DomainError, Unit>> SendOvasFeedNotifications(string seasonInfo, CancellationToken token)
    {
        return _userGetter.GetAvailableUsersData(token)
            .MapAsync(users => users.ConvertAll(user => new OvasCheckFeedMatchesEvent(user.Email, user.UserId, seasonInfo)))
            .BindAsync(notification => SendNotifications(notification, token));
    }

    private async Task<Either<DomainError, Unit>> SendNotifications(ImmutableList<OvasCheckFeedMatchesEvent> notifications,
        CancellationToken token)
    {
        var process = notifications.Select(notification => _domainPostman.SendMessage(notification, token)).ToArray();
        var results = await Task.WhenAll(process);
        return results.FlattenResults().Map(_ => unit);
    }
}