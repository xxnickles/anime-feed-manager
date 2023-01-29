using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Application;

public readonly record struct SubscriptionCollection(string Subscriber, IEnumerable<string> Series);

public readonly record struct ShortSeriesSubscriptionCollection(string Subscriber, IEnumerable<ShortSeries> Series);