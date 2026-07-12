using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Feeds.Matching;

public sealed record ParsedRelease(string CleanTitle, ReleaseContent Content, bool IsBdRemux);

/// <summary>
/// Parses a raw fansub release title into a clean series title plus content-type/episode info.
/// Fansub naming isn't standardized across groups — this handles the dominant
/// "[Group] Title - NN [tech info]" / "[Group] Title (NN-MM) [tech info]" shapes observed across
/// SubsPlease and Erai-raws, the two groups covering the vast majority of Crunchyroll-simulcast
/// releases. A title with its own leading number, or an unusual group's format, can still slip
/// through as a false MovieOrOva or mismatched episode — this is a heuristic, not a guarantee.
/// </summary>
internal static partial class ReleaseTitleParser
{
    [GeneratedRegex(@"^\[[^\]]+\]\s*")]
    private static partial Regex GroupTagPattern();

    [GeneratedRegex(@"\.(mkv|mp4|avi|ass)$", RegexOptions.IgnoreCase)]
    private static partial Regex FileExtensionPattern();

    [GeneratedRegex(@"\b(BD|BDRip|Blu-?Ray|REMUX)\b", RegexOptions.IgnoreCase)]
    private static partial Regex BdRemuxPattern();

    // Non-greedy title capture: backtracks past any " - " that belongs to the title itself
    // (e.g. "EXCEEDS - Gun Blaze Vengeance") because the digits requirement right after the
    // separator only succeeds at the genuine episode marker.
    [GeneratedRegex(@"^(?<title>.+?)\s*\((?<start>\d{1,4})-(?<end>\d{1,4})\)")]
    private static partial Regex BatchPattern();

    [GeneratedRegex(@"^(?<title>.+?)\s-\s(?<episode>\d{1,4})(?:v\d+)?(?=\s*[\[(]|$)")]
    private static partial Regex EpisodePattern();

    [GeneratedRegex(@"^[^\[(]+")]
    private static partial Regex LeadingTitlePattern();

    public static ParsedRelease Parse(string rawTitle)
    {
        var core = FileExtensionPattern().Replace(GroupTagPattern().Replace(rawTitle, ""), "").Trim();
        var isBdRemux = BdRemuxPattern().IsMatch(core);

        if (BatchPattern().Match(core) is {Success: true} batch)
        {
            return new ParsedRelease(
                batch.Groups["title"].Value.Trim(),
                new ReleaseContent.Batch(int.Parse(batch.Groups["start"].Value), int.Parse(batch.Groups["end"].Value)),
                isBdRemux);
        }

        if (EpisodePattern().Match(core) is {Success: true} episode)
        {
            return new ParsedRelease(
                episode.Groups["title"].Value.Trim(),
                new ReleaseContent.SingleEpisode(int.Parse(episode.Groups["episode"].Value)),
                isBdRemux);
        }

        var title = LeadingTitlePattern().Match(core) is {Success: true} leading ? leading.Value : core;
        return new ParsedRelease(title.Trim(), new ReleaseContent.NonNumbered(), isBdRemux);
    }
}
