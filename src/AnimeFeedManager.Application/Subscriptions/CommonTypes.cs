using System.Collections.Generic;

namespace AnimeFeedManager.Application.Subscriptions;

public record InterestedSeriesItem(string UserId, string InterestedAnime);

public record InterestedToSubscription(string UserId, string InterestedTitle, string FeedTitle);

public record SubscriptionCollection(string SubscriptionId, IEnumerable<string> SubscribedAnimes);