namespace AnimeFeedManager.Features.Tv.Subscriptions.Types;

public readonly record struct InterestedToSubscription(string UserId, string FeedTitle, string InterestedTitle);

public readonly record struct UserAutoSubscription(string UserId);