using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public record Summary(int Changes);

public record AutoSubscriptionProcess(string SeriesId, ImmutableList<SubscriptionStorage> InterestedSeries);
