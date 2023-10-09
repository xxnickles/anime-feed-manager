namespace AnimeFeedManager.Features.Tv.Subscriptions.Types;

public record InterestedToSubscription(string UserId, string FeedTitle, string InterestedTitle);

public readonly record struct UserAutoSubscription(string UserId);