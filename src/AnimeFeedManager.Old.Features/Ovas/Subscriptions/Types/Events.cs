using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.Types;

public record OvasCheckFeedMatchesEvent(string UserEmail, string UserId, string PartitionKey) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-feed-matches-process";
}


public record CompleteOvaSubscriptionEvent(OvasSubscriptionStorage OvaInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ova-subscription-completer";
}

public record OvaFeedRemovedEvent(string Tile) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ova-feed-removed-process";
}