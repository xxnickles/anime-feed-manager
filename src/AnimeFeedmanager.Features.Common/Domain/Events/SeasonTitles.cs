namespace AnimeFeedManager.Features.Common.Domain.Events;

public record UpdateSeasonTitlesRequest(ImmutableList<string> Titles);