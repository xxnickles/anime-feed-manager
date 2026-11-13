namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

/// <summary>The season data reached an update step that only takes <paramref name="Expected"/>.</summary>
public sealed record SeasonNotUpdatable(Type Received, string Expected)
    : DomainError($"Season data {Received.Name} can't be updated here: expected {Expected}")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
