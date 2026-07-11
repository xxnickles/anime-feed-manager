namespace AnimeFeedManager.Features.Feeds.Entities;

public enum ReleaseContentType
{
    Episode,
    Batch,
    MovieOrOva,
    BdRemux
}

/// <summary>
/// A detected release, handed off to a future notification-delivery process — the sole
/// interface between collection and delivery. Partitioned by <see cref="SeriesId"/>, alongside
/// that series' <see cref="SeriesClassification"/>. <see cref="Confirmed"/> distinguishes a
/// Nyaa-confirmed release from an AniList-clock best-effort one; <see cref="Platforms"/> is
/// copied from <see cref="SeriesClassification"/> at detection time (a historical snapshot, not
/// a live join) so delivery can say "also on Crunchyroll" or, for an unconfirmed detection,
/// "expected on Netflix" without an extra lookup.
/// </summary>
public sealed record ReleaseDetected : FeedsDocument
{
    public int SeriesId { get; }

    public required ReleaseContentType ContentType { get; init; }
    public int? Episode { get; init; }
    public int? EpisodeRangeEnd { get; init; }
    public required bool Confirmed { get; init; }
    public FeedsPlatform[] Platforms { get; init; } = [];

    public string? SourceTitle { get; init; }
    public string? SourceLink { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }

    public ReleaseDetected(int seriesId)
    {
        SeriesId = seriesId;
        PartitionKey = seriesId.ToString();
    }
}
