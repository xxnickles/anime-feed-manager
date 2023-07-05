namespace AnimeFeedManager.Features.Domain.Types;

public abstract record SeasonSelector;

public record Latest : SeasonSelector;

public record BySeason(SimpleSeasonInfo SeasonInfo) : SeasonSelector;