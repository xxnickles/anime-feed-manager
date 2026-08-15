using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Feeds.Classification;

/// <summary>
/// Builds a series' <see cref="SeriesClassification"/> from its licensed platforms. The platform
/// list is monotonic: an empty fresh read (a 504 reads as "no platforms") never overwrites
/// previously-known platforms — only real data replaces real data.
/// </summary>
internal static class SeriesClassifier
{
    public static SeriesClassification Classify(
        int seriesId,
        ImmutableArray<JikanStreamingEntry> platforms,
        FeedsPlatform[]? previousPlatforms = null)
    {
        var freshPlatforms = platforms.Select(p => new FeedsPlatform(p.Name, p.Url ?? string.Empty)).ToArray();

        return new SeriesClassification(seriesId)
        {
            Platforms = freshPlatforms.Length > 0 ? freshPlatforms : previousPlatforms ?? []
        };
    }
}
