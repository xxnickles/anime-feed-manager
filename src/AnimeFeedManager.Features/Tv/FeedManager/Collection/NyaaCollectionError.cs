using AnimeFeedManager.Features.Scrapping;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

public union NyaaCollectionError(StorageError, ScrapeError)
    : IWidensFrom<NyaaCollectionError, StorageError>, IWidensFrom<NyaaCollectionError, ScrapeError>
{
    static NyaaCollectionError IWidensFrom<NyaaCollectionError, StorageError>.Widen(StorageError error) => error;
    static NyaaCollectionError IWidensFrom<NyaaCollectionError, ScrapeError>.Widen(ScrapeError error) => error;
}
