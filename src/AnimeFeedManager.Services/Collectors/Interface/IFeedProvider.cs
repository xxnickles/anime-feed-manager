using System.Collections.Immutable;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface IFeedProvider
{
    Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution);
}