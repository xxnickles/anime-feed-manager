using AnimeFeedManager.Features.Common.Types;
using AnimeFeedManager.Features.Domain;
using AnimeFeedManager.Features.Domain.Errors;

namespace AnimeFeedManager.Features.Tv.Feed.IO;

public interface IFeedProvider {
    Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution);
}