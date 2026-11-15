using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

[Widens]
public partial union AutoSubscriptionError(StorageError, DeliveryError);

public static class AutoSubscription
{
    public static Task<Result<Summary, AutoSubscriptionError>> TryToSubscribe(
        string seriesId,
        string feedTitle,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token)
    {
        return StartProcess(seriesId, feedTitle, clientFactory.TableStorageTvInterestedBySeries, token)
            .WithOperationName(nameof(TryToSubscribe))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId)
            .StoreChanges(clientFactory.TableStorageTvSubscriptionsUpdater, token)
            .SendEvents(seriesId, domainPostman, token);
    }


    internal static Task<Result<AutoSubscriptionProcess, AutoSubscriptionError>> StartProcess(
        string seriesId,
        string feedTitle,
        TvInterestedBySeries seriesGetter,
        CancellationToken token) => seriesGetter(seriesId, token)
        .Map(series =>
            new AutoSubscriptionProcess(seriesId, series.Select(s => Subscribe(s, feedTitle)).ToImmutableArray()))
        .MapError(error => (AutoSubscriptionError)error);

    extension(Task<Result<AutoSubscriptionProcess, AutoSubscriptionError>> process)
    {
        internal Task<Result<AutoSubscriptionProcess, AutoSubscriptionError>> StoreChanges(TvSubscriptionsUpdater updater,
            CancellationToken token)
        {
            return process.Bind(p => updater(p.InterestedSeries, token)
                .Map(_ => p));
        }

        internal Task<Result<Summary, AutoSubscriptionError>> SendEvents(string seriesId,
            DomainCollectionSender domainPostman,
            CancellationToken token)
        {
            return process.BindWhen(
                    data => domainPostman([GetEvent(data)], token).Map(_ => data),
                    data => data.InterestedSeries.Length > 0)
                .Map(data => new Summary(data.InterestedSeries.Length))
                .SendOnError(domainPostman, [GetErrorEvent(seriesId)], token);
        }
    }


    private static SystemEvent GetEvent(AutoSubscriptionProcess data) =>
        new(
            TargetConsumer.Admin(),
            EventTarget.LocalStorage,
            EventType.Completed,
            new AutoSubscriptionResult(data.SeriesId,
                data.InterestedSeries.Length).AsEventPayload());


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