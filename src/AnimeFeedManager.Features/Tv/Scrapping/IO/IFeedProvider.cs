using AnimeFeedManager.Features.Common.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public interface IFeedProvider {
    Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution);
}