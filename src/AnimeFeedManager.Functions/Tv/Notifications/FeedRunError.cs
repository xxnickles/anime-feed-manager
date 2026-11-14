using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;

namespace AnimeFeedManager.Functions.Tv.Notifications;

/// <summary>The feed notification run: collects nyaa releases, backfills a window gap, notifies users, stamps the poll.</summary>
internal union FeedRunError(StorageError, ScrapeError, DeliveryError)
    : IWidensFrom<FeedRunError, StorageError>,
      IWidensFrom<FeedRunError, DeliveryError>,
      IWidensFrom<FeedRunError, NyaaCollectionError>,
      IWidensFrom<FeedRunError, FeedProcessError>
{
    static FeedRunError IWidensFrom<FeedRunError, StorageError>.Widen(StorageError error) => error;
    static FeedRunError IWidensFrom<FeedRunError, DeliveryError>.Widen(DeliveryError error) => error;

    static FeedRunError IWidensFrom<FeedRunError, NyaaCollectionError>.Widen(NyaaCollectionError error) => error switch
    {
        StorageError storage => storage,
        ScrapeError scrape => scrape
    };

    static FeedRunError IWidensFrom<FeedRunError, FeedProcessError>.Widen(FeedProcessError error) => error switch
    {
        StorageError storage => storage,
        DeliveryError delivery => delivery
    };
}
