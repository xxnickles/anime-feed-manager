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
/// shouldn't erase proof the series is actually reachable via Nyaa. The platform list itself is
/// monotonic the same way: an empty fresh read (a 504 reads as "no platforms") never overwrites
/// previously-known platforms — only real data replaces real data.
/// </remarks>
internal static class SeriesClassifier
{
    private static readonly FrozenSet<string> TrackablePlatforms =
        FrozenSet.ToFrozenSet(["Crunchyroll"], StringComparer.OrdinalIgnoreCase);

    public static SeriesClassification Classify(
        int seriesId,
        ImmutableArray<JikanStreamingEntry> platforms,
        SeriesTrackability previousTrackability = SeriesTrackability.Untrackable,
        FeedsPlatform[]? previousPlatforms = null)
    {
        var computed = platforms.Any(p => TrackablePlatforms.Contains(p.Name))
            ? SeriesTrackability.Trackable
            : SeriesTrackability.Untrackable;

        var trackability = previousTrackability == SeriesTrackability.Trackable
            ? SeriesTrackability.Trackable
            : computed;

        var freshPlatforms = platforms.Select(p => new FeedsPlatform(p.Name, p.Url ?? string.Empty)).ToArray();

        return new SeriesClassification(seriesId)
        {
            Trackability = trackability,
            Platforms = freshPlatforms.Length > 0 ? freshPlatforms : previousPlatforms ?? []
        };
    }

    /// <summary>
    /// A confirmed Nyaa match is direct proof of trackability — stronger evidence than the
    /// platform-list inference. Used by the collection job to promote a series the moment a
    /// release actually lands, without waiting for the next classification pass.
    /// </summary>
    public static SeriesClassification MarkTrackable(SeriesClassification current) =>
        current with { Trackability = SeriesTrackability.Trackable };
}
