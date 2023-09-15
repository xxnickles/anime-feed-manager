using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.State.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public class AutomatedSubscriptionProcessor
{
    private record struct Payload(
        UserId UserId,
        NoEmptyString SubscriptionTitle,
        NoEmptyString InterestedTitle);

    private readonly IAddTvSubscription _addTvSubscription;
    private readonly IRemoveInterestedSeries _removeInterestedSeries;
    private readonly IStateUpdater _stateUpdater;
    private readonly IStoreNotification _storeNotification;

    public AutomatedSubscriptionProcessor(
        IAddTvSubscription addTvSubscription,
        IRemoveInterestedSeries removeInterestedSeries,
        IStateUpdater stateUpdater,
        IStoreNotification storeNotification
    )
    {
        _addTvSubscription = addTvSubscription;
        _removeInterestedSeries = removeInterestedSeries;
        _stateUpdater = stateUpdater;
        _storeNotification = storeNotification;
    }

    public async Task<Either<DomainError, Unit>> Process(StateWrap<InterestedToSubscription> subscription,
        CancellationToken token)
    {
        var processResult = await Parse(subscription.Payload)
            .BindAsync(payload => ProcessSubscription(payload, token));

        return await _stateUpdater.Update(
            processResult,
            new StateChange(subscription.StateId, NotificationTarget.Tv, subscription.Payload.FeedTitle),
            token).BindAsync(state => CreateNotification(state, subscription.Payload.UserId, token));
    }


    private Task<Either<DomainError, Unit>> ProcessSubscription(Payload payload, CancellationToken token)
    {
        return _addTvSubscription.Subscribe(payload.UserId, payload.SubscriptionTitle, token)
            .BindAsync(_ => _removeInterestedSeries.Remove(payload.UserId, payload.InterestedTitle, token));
    }

    private static Either<DomainError, Payload> Parse(InterestedToSubscription subscription)
    {
        return (UserIdValidator.Validate(subscription.UserId),
                NoEmptyString.FromString(subscription.FeedTitle)
                    .ToValidation(ValidationError.Create("Feed Title", "Feed title is empty")),
                NoEmptyString.FromString(subscription.InterestedTitle)
                    .ToValidation(ValidationError.Create("Interested Title", "Interested Title is empty")))
            .Apply((userId, subscriptionTitle, interestedTitle) =>
                new Payload(userId, subscriptionTitle, interestedTitle))
            .ValidationToEither();
    }

    private Task<Either<DomainError, Unit>> CreateNotification(CurrentState currentState, string userId,
        CancellationToken token)
    {
        if (!currentState.ShouldNotify) return Task.FromResult(Right<DomainError, Unit>(unit));

        var notification = new AutomatedSubscriptionProcessNotification(
            TargetAudience.User,
            NotificationType.Update,
            currentState.Items.Split(','),
            $"Automated subscription has been completed with {currentState.Completed} subscriptions added. {currentState.Errors} were found");
        return _storeNotification.Add(IdHelpers.GetUniqueId(), userId, NotificationTarget.Tv, NotificationArea.Feed,
            notification, token);
    }
}