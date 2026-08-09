namespace AnimeFeedManager.Features.Feeds.Entities;

public enum ReleaseContentType
{
    Unknown,
    Episode,
    Batch,
    MovieOrOva,
    BdRemux
}

/// <summary>Whether the notification-dispatch pipeline has processed this release — not per-user delivery state (later concern).</summary>
public enum ReleaseDetectedStatus
{
    Pending,
    Processed
}

/// <summary>
/// A detected release, handed off to a future notification-delivery process — the sole
/// interface between collection and delivery. Partitioned by <see cref="SeriesId"/>, alongside
/// that series' <see cref="SeriesClassification"/>. <see cref="Confirmed"/> distinguishes a
/// Nyaa-confirmed release from an AniList-clock best-effort one; <see cref="Platforms"/> is
/// copied from <see cref="SeriesClassification"/> at detection time (a historical snapshot, not
/// a live join) so delivery can say "also on Crunchyroll" or, for an unconfirmed detection,
/// "expected on Netflix" without an extra lookup.
/// <see cref="Ttl"/> expires undispatched entries after ~48h — only takes effect because the
/// shared "feeds" container has <c>DefaultTimeToLive</c> enabled (see AppHost.cs); the other
/// document types sharing that container never set <c>ttl</c>, so they're unaffected.
/// </summary>
public sealed record ReleaseDetected : SeriesFeedsDocument
{
    public const int DefaultTtlSeconds = 60 * 60 * 48;

    public ReleaseContentType ContentType { get; init; } = ReleaseContentType.Unknown;
    public int? Episode { get; init; }
    public int? EpisodeRangeEnd { get; init; }
    public bool Confirmed { get; init; }
    public FeedsPlatform[] Platforms { get; init; } = [];
    public ReleaseDetectedStatus Status { get; init; } = ReleaseDetectedStatus.Pending;

    public string? SourceTitle { get; init; }
    public string? SourceLink { get; init; }
    public DateTimeOffset DetectedAt { get; init; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; init; } = DefaultTtlSeconds;

    public ReleaseDetected(int seriesId) : base(seriesId)
    {
    }
}
