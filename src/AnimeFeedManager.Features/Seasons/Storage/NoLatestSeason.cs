namespace AnimeFeedManager.Features.Seasons.Storage;

/// <summary>Storage holds no season to treat as the latest one.</summary>
public sealed record NoLatestSeason() : DomainError("There is no latest season data in storage")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
