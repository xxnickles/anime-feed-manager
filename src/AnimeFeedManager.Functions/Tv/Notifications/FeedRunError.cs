using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;

namespace AnimeFeedManager.Functions.Tv.Notifications;

/// <summary>The feed notification run: collects nyaa releases, backfills a window gap, notifies users, stamps the poll.</summary>
[Widens<NyaaCollectionError, FeedProcessError>]
internal partial union FeedRunError(StorageError, ScrapeError, DeliveryError);
