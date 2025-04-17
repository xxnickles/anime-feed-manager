namespace AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;

public readonly record struct SubscriptionCollection(Email SubscriberEmail, ImmutableList<NoEmptyString> Series);

public readonly record struct CollectedNotificationResult(byte SeriesCount, string Subscriber);