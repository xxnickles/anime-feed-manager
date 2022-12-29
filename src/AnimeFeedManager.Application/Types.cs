namespace AnimeFeedManager.Application;

public record struct SubscriptionCollection(string Subscriber, IEnumerable<string> Series);