using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class AutoSubscription
{
    public static Task<Result<Summary>> TryToSubscribe(
        string seriesId,
        string feedTitle,
        ITableClientFactory clientFactory,
        IDomainPostman domainPostman,
        CancellationToken token)
    {
        return StartProcess(seriesId, feedTitle, clientFactory.TableStorageTvInterestedBySeries(), token)
            .StoreChanges(clientFactory.TableStorageTvSubscriptionsUpdater(), token)
            .SendEvents(seriesId, domainPostman, token)
            .MapError(error => error
                .WithOperationName(nameof(TryToSubscribe))
                .WithLogProperty("SeriesId", seriesId));
    }


    internal static Task<Result<AutoSubscriptionProcess>> StartProcess(
        string seriesId,
        string feedTitle,
        TvInterestedBySeries seriesGetter,
        CancellationToken token) => seriesGetter(seriesId, token)
        .Map(series => new AutoSubscriptionProcess(seriesId, series.ConvertAll(s => Subscribe(s, feedTitle))));

    extension(Task<Result<AutoSubscriptionProcess>> process)
    {
        internal Task<Result<AutoSubscriptionProcess>> StoreChanges(TvSubscriptionsUpdater updater, CancellationToken token)
        {
            return process.Bind(p => updater(p.InterestedSeries, token)
                .Map(_ => p));
        }

        internal Task<Result<Summary>> SendEvents(string seriesId,
            IDomainPostman domainPostman,
            CancellationToken token)
        {
            return process.Bind(data => domainPostman.SendMessage(GetEvent(data), token)
                    .Map(_ => new Summary(data.InterestedSeries.Count)))
                .MapError(error =>
                    domainPostman.SendMessage(GetErrorEvent(seriesId), token)
                        .MatchToValue(_ => error, e => e));
        }
    }


    private static SystemEvent GetEvent(AutoSubscriptionProcess data) =>
        new(
            TargetConsumer.Admin(),
            EventTarget.LocalStorage,
            EventType.Completed,
            new AutoSubscriptionResult(data.SeriesId,
                data.InterestedSeries.Count).AsEventPayload());


    private static SystemEvent GetErrorEvent(string seriesId) =>
        new(
            TargetConsumer.Admin(),
            EventTarget.LocalStorage,
            EventType.Error,
            new AutoSubscriptionResult(seriesId, 0).AsEventPayload());


    private static SubscriptionStorage Subscribe(SubscriptionStorage subscription, string feedTitle)
    {
        subscription.Type = nameof(SubscriptionType.Subscribed);
        subscription.SeriesFeedTitle = feedTitle;
        return subscription;
    }
}