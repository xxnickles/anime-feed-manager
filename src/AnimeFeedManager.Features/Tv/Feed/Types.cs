using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Feed;

public record FeedProcessData(DailySeriesFeed[] DailyFeed, UserActiveSubscriptions[] Subscriptions);

public record FeedProcessSummary(int UsersToNotify);

