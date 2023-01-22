namespace AnimeFeedManager.Application;

public readonly record struct SubscriptionCollection(string Subscriber, IEnumerable<string> Series);