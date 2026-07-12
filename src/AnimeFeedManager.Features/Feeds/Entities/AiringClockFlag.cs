namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// Idempotency marker for the AniList airing-clock job — one document per series once it has
/// been flagged at least once, partitioned by <see cref="SeriesId"/>. Written only by that job,
/// for <see cref="SeriesTrackability.Untrackable"/> series only. The document's existence is the
/// "has been flagged" signal; <see cref="LastFlaggedEpisode"/> holds the highest episode flagged.
/// AniList is queried fresh every run (one batch call) — this document is the only state we
/// persist, not a cache of AniList's answer.
/// </summary>
public sealed record AiringClockFlag : FeedsDocument
{
    public int SeriesId { get; }

    public int LastFlaggedEpisode { get; init; }

    public AiringClockFlag(int seriesId)
    {
        SeriesId = seriesId;
        Id = $"airing-clock-flag:{seriesId}";
        PartitionKey = seriesId.ToString();
    }
}
