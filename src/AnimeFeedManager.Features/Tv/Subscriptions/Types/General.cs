namespace AnimeFeedManager.Features.Tv.Subscriptions.Types;

public readonly record struct SubscriptionCollection(Email SubscriberEmail, ImmutableList<NoEmptyString> Series);