namespace AnimeFeedManager.Features.Notifications;

/// <summary>Background + text color for a platform pill in the notification email.</summary>
public sealed record PlatformColor(string Background, string Text);

/// <summary>
/// Brand color per platform pill. Lookup is case-insensitive against <c>FeedsPlatform.Name</c>
/// (free text from Jikan) plus "Nyaa" for the synthesized search pill. An unrecognized name falls
/// back to a neutral color rather than failing — Jikan can surface platforms this list doesn't know.
/// </summary>
public static class PlatformColors
{
    private static readonly PlatformColor Fallback = new("#D9CBB8", "#3B2A1E");

    private static readonly IReadOnlyDictionary<string, PlatformColor> Known =
        new Dictionary<string, PlatformColor>(StringComparer.OrdinalIgnoreCase)
        {
            ["Crunchyroll"] = new PlatformColor("#FF9C2E", "#3B2A1E"),
            ["Netflix"] = new PlatformColor("#E85554", "#FFFFFF"),
            ["Nyaa"] = new PlatformColor("#7FB56E", "#3B2A1E"),
            ["HIDIVE"] = new PlatformColor("#8FD3E8", "#3B2A1E")
        };

    public static PlatformColor For(string platformName) =>
        Known.GetValueOrDefault(platformName, Fallback);
}
