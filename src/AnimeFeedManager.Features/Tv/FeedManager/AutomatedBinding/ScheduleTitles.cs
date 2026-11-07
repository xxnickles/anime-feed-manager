using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

internal static partial class ScheduleTitles
{
    /// <summary>Lowercase, punctuation as spaces, and "2nd Season" / "Season 2" as <c>s2</c>, SubsPlease's spelling.</summary>
    internal static string Normalize(string title)
    {
        var words = Whitespace().Replace(NonWord().Replace(title.ToLowerInvariant(), " "), " ").Trim();
        return SeasonNumber().Replace(OrdinalSeason().Replace(words, "s$1"), "s$1");
    }

    [GeneratedRegex(@"[^\p{L}\p{N}\s]")]
    private static partial Regex NonWord();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"\b(\d+)(?:st|nd|rd|th) season\b")]
    private static partial Regex OrdinalSeason();

    [GeneratedRegex(@"\bseason (\d+)\b")]
    private static partial Regex SeasonNumber();
}
