using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.Types;

public record OvasCheckFeedMatches(string UserEmail, string UserId, string PartitionKey) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-feed-matches-process";
}


public record CompleteOvaSubscription(OvasSubscriptionStorage OvaInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ova-subscription-completer";
}