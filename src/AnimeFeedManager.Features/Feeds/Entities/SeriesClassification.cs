namespace AnimeFeedManager.Features.Feeds.Entities;

public readonly record struct FeedsPlatform(string Name, string Url);

/// <summary>
/// Per-series classification, written only by the weekly library-import cadence — one
/// document per series, partitioned by <see cref="SeriesId"/>. <see cref="Platforms"/> carries
/// into <see cref="ReleaseDetected"/> as notification enrichment ("also on Crunchyroll").
/// Clock-eligibility for the airing-clock job is no longer tracked here — it's a runtime check
/// against whether a <c>NyaaConfirmation</c> exists for the series, not a stored classification.
/// </summary>
public sealed record SeriesClassification : SeriesFeedsDocument
{
    public FeedsPlatform[] Platforms { get; init; } = [];

    public SeriesClassification(int seriesId) : base(seriesId)
    {
        Id = $"classification:{seriesId}";
    }
}
