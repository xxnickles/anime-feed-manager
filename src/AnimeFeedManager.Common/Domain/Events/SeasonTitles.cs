namespace AnimeFeedManager.Common.Domain.Events;

public record UpdateSeasonTitlesRequest(ImmutableList<string> Titles);