namespace AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

/// <summary>One series' next-episode airing schedule, as returned by <see cref="AniList.IAniListClient"/>.</summary>
public sealed record AniListEpisodeClock(int MalId, int NextEpisode, DateTimeOffset NextEpisodeAiringAt);
