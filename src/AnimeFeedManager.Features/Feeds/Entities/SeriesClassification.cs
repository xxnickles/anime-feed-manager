namespace AnimeFeedManager.Features.Feeds.Entities;

public enum SeriesTrackability
{
    Trackable,
    Untrackable
}

public readonly record struct FeedsPlatform(string Name, string Url);

/// <summary>
/// Per-series classification, written only by the weekly library-import cadence — one
/// document per series, partitioned by <see cref="SeriesId"/>. <see cref="Trackability"/> is a
/// hard gate: <see cref="SeriesTrackability.Trackable"/> series (any fansub-covered platform in
/// <see cref="Platforms"/>) rely solely on the Nyaa collection job; <see cref="SeriesTrackability.Untrackable"/>
/// series rely solely on the AniList airing clock. <see cref="Platforms"/> also carries into
/// <see cref="ReleaseDetected"/> as notification enrichment ("also on Crunchyroll").
/// </summary>
public sealed record SeriesClassification : SeriesFeedsDocument
{
    public SeriesTrackability Trackability { get; init; } = SeriesTrackability.Untrackable;
    public FeedsPlatform[] Platforms { get; init; } = [];

    public SeriesClassification(int seriesId) : base(seriesId)
    {
        Id = $"classification:{seriesId}";
    }
}
