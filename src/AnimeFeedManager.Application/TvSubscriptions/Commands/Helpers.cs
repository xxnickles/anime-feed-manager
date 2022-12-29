using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.TvSubscriptions.Commands;

internal static class Helpers
{
    internal static SubscriptionStorage MapToStorage(Subscription subscription)
    {
        return new SubscriptionStorage
        {
            PartitionKey = subscription.Subscriber.Value.UnpackOption(string.Empty),
            RowKey = subscription.AnimeId.Value.UnpackOption(string.Empty)
        };
    }
    internal static InterestedStorage MapToStorage(InterestedSeries interested)
    {
        return new InterestedStorage
        {
            PartitionKey = interested.Subscriber.Value.UnpackOption(string.Empty),
            RowKey = interested.AnimeId.Value.UnpackOption(string.Empty)
        };
    }
}