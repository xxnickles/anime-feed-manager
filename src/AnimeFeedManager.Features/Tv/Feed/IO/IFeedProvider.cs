namespace AnimeFeedManager.Features.Tv.Feed.IO
{
    public interface IFeedProvider {
        Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution);
    }
}