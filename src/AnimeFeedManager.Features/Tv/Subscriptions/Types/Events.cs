using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Types;

public record InterestedToSubscription(string UserId, string FeedTitle, string InterestedTitle)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "auto-subscriptions-process";
}

public record UserAutoSubscription(string UserId) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "user-auto-subscription";
}