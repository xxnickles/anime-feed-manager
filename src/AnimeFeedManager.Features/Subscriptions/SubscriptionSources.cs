namespace AnimeFeedManager.Features.Subscriptions;

/// <summary>
/// Stable source identifier for Subscriptions' <c>SubscriptionEvent</c> persistence — shared by
/// both Subscribe and Unsubscribe, since <c>RecentSubscriptionEventsLoader</c> partitions its
/// query by this value.
/// </summary>
internal static class SubscriptionSources
{
    public const string SubscriptionActivity = "SubscriptionActivity";
}
