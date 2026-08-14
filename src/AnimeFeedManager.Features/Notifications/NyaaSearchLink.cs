using AnimeFeedManager.Features.Feeds.Sources.Nyaa;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// Builds a Nyaa search-results link for a release — not a direct link to the specific matched
/// torrent (<c>ReleaseDetected.SourceLink</c>), which could be low-res or the wrong fansub group.
/// Pointing at a search instead lets the recipient pick their own release. Uses the same
/// category/filter as the collection job (<see cref="NyaaOptions"/>) so the link only ever
/// surfaces what the automated matcher itself would trust.
/// </summary>
public static class NyaaSearchLink
{
    public static string Build(NyaaOptions options, string title, int? episode)
    {
        var query = episode is { } ep ? $"{title} {ep}" : title;
        return $"{options.BaseUrl}?f={options.Filter}&c={options.Category}&q={Uri.EscapeDataString(query)}";
    }
}
