using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Outcome of reconciling an AniList episode clock against a series' <see cref="AiringClockFlag"/> —
/// closed so consumers pattern-match instead of branching on nulls.
/// </summary>
internal abstract record AiringClockResult
{
    private AiringClockResult()
    {
    }

    public sealed record Flagged(AiringClockFlag UpdatedFlag, int EpisodeStart, int EpisodeEnd) : AiringClockResult;

    public sealed record NoChange : AiringClockResult;
}

/// <summary>
/// AniList only exposes the *next* airing episode, not a full history, so the last-aired
/// episode is always <c>NextEpisode - 1</c>. Anything already flagged at or above that number
/// is stale news; anything higher is newly aired since the last run (a range, not just one
/// episode, when a run is missed and multiple episodes have aired in between).
/// </summary>
internal static class AiringClockReconciler
{
    public static AiringClockResult Reconcile(AniListEpisodeClock clock, AiringClockFlag? previous)
    {
        var lastAired = clock.NextEpisode - 1;
        var previouslyFlagged = previous?.LastFlaggedEpisode ?? 0;

        if (lastAired <= previouslyFlagged)
            return new AiringClockResult.NoChange();

        var updatedFlag = (previous ?? new AiringClockFlag(clock.MalId)) with { LastFlaggedEpisode = lastAired };
        return new AiringClockResult.Flagged(updatedFlag, previouslyFlagged + 1, lastAired);
    }
}
