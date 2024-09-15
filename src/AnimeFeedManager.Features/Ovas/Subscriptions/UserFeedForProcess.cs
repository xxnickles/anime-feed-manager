using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions;

public readonly record struct OvasUserFeed(
    ImmutableList<FeedProcessedOva> Feed,
    ImmutableList<OvasSubscriptionStorage> Subscriptions);

public sealed class UserOvasFeedForProcess
{
    private readonly IGetProcessedOvas _getProcessedOvas;
    private readonly IGetOvasSubscriptions _getOvasSubscriptions;

    public UserOvasFeedForProcess(IGetProcessedOvas getProcessedOvas, IGetOvasSubscriptions getOvasSubscriptions)
    {
        _getProcessedOvas = getProcessedOvas;
        _getOvasSubscriptions = getOvasSubscriptions;
    }

    public Task<Either<DomainError, OvasUserFeed>> GetFeedForProcess(UserId userId, PartitionKey partitionKey,
        CancellationToken token)
    {
        return _getProcessedOvas.GetForSeason(partitionKey, token)
            .BindAsync(feeds => _getOvasSubscriptions.GetCompleteSubscriptions(userId, token)
                .MapAsync(subscriptions => new OvasUserFeed(feeds, subscriptions)))
            .MapAsync(Filter);
    }

    private static OvasUserFeed Filter(OvasUserFeed userFeed)
    {
        var feedTitles = userFeed.Feed.Select(f => f.SeriesTitle);
        var subscriptionsTitles = userFeed.Subscriptions.Select(s => s.RowKey);
        var matches = feedTitles.Intersect(subscriptionsTitles);

        if (!matches.Any())
            return new OvasUserFeed(ImmutableList<FeedProcessedOva>.Empty,
                ImmutableList<OvasSubscriptionStorage>.Empty);

        return new OvasUserFeed(userFeed.Feed.Where(f => matches.Contains(f.SeriesTitle)).ToImmutableList(),
            userFeed.Subscriptions.Where(s => matches.Contains(s.RowKey)).ToImmutableList());
    }
}