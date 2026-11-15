using AnimeFeedManager.Features.Scrapping;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

[Widens]
public partial union NyaaCollectionError(StorageError, ScrapeError);
