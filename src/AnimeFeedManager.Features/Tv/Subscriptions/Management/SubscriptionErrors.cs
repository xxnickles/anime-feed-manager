namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

/// <summary>The feed title keys notifications, so a series without one can't be subscribed to.</summary>
public sealed record NoFeed(string Series) : DomainError($"{Series} has no feed to subscribe to")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}

public sealed record AlreadySubscribed(string Series) : DomainError($"You are already subscribed to {Series}")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
