namespace AnimeFeedManager.Application.TvSubscriptions;

public record InterestedSeriesItem(string UserId, string InterestedAnime);

public record InterestedToSubscription(string UserId, string InterestedTitle, string FeedTitle);

public record SubscriptionCollection(string SubscriptionId, IEnumerable<string> SubscribedAnimes);