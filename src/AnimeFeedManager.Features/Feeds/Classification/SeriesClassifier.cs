using System.Collections.Frozen;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Feeds.Classification;

/// <summary>
/// Derives a series' <see cref="SeriesTrackability"/> from its licensed platforms. Fansub groups
/// source almost exclusively from Crunchyroll's simulcast stream — a series with no
/// fansub-covered platform will structurally never appear on Nyaa, regardless of how long we wait.
/// </summary>
/// <remarks>
/// Trackability is monotonic: once <see cref="SeriesTrackability.Trackable"/>, it never reverts to
/// <see cref="SeriesTrackability.Untrackable"/>, regardless of what a fresh Jikan read says. A real
/// Nyaa confirmation (or a prior Trackable classification) is stronger evidence than the
/// platform-list inference — Jikan's data can flicker or a platform can be delisted, but that
/// shouldn't erase proof the series is actually reachable via Nyaa.
/// </remarks>
internal static class SeriesClassifier
{
    private static readonly FrozenSet<string> TrackablePlatforms =
        FrozenSet.ToFrozenSet(["Crunchyroll"], StringComparer.OrdinalIgnoreCase);

    public static SeriesClassification Classify(
        int seriesId,
        ImmutableArray<JikanStreamingEntry> platforms,
        SeriesTrackability previousTrackability = SeriesTrackability.Untrackable)
    {
        var computed = platforms.Any(p => TrackablePlatforms.Contains(p.Name))
            ? SeriesTrackability.Trackable
            : SeriesTrackability.Untrackable;

        var trackability = previousTrackability == SeriesTrackability.Trackable
            ? SeriesTrackability.Trackable
            : computed;

        return new SeriesClassification(seriesId)
        {
            Trackability = trackability,
            Platforms = [..platforms.Select(p => new FeedsPlatform(p.Name, p.Url ?? string.Empty))]
        };
    }
}
