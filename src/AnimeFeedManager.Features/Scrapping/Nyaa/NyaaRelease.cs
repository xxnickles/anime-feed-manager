using System.Globalization;
using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Scrapping.Nyaa;

public abstract record NyaaEpisode(string Label);

public sealed record RegularEpisode(int Number, string Label) : NyaaEpisode(Label);

/// <summary>Episode 0 or a decimal (12.5): catalogued and notified, never counted toward the total.</summary>
public sealed record SpecialEpisode(decimal Number, string Label) : NyaaEpisode(Label);

/// <param name="Title">The group's release title, which drives matching.</param>
/// <param name="Revised">A <c>v2+</c> or <c>REPACK</c> of an episode already released.</param>
/// <param name="Variant">Tags with the per-episode tokens removed; the exact-match signature.</param>
public sealed record NyaaRelease(string Group, string Title, NyaaEpisode Episode, bool Revised, string Variant);

public static partial class NyaaReleaseTitle
{
    /// <summary>Parses <c>[Group] Title - NN(vN) tags</c> or <c>[Group] Title (-) SxxEyy tags</c>; batches and titles without an episode are warnings.</summary>
    public static Result<NyaaRelease> Parse(string title)
    {
        if (BatchMarker().IsMatch(title))
            return Warning.Create($"Skipping nyaa release {title}: batch");

        var match = Release().Match(title.Trim());
        if (!match.Success)
            return Warning.Create($"Skipping nyaa release {title}: no [Group] prefix or episode number");

        var tags = match.Groups["tags"].Value;
        var version = match.Groups["version"].Success ? int.Parse(match.Groups["version"].Value) : 1;

        return new NyaaRelease(
            Group: match.Groups["group"].Value.Trim(),
            Title: match.Groups["title"].Value.Trim(),
            Episode: Episode(match.Groups["episode"].Value, match.Groups["label"].Value.ToUpperInvariant()),
            Revised: version > 1 || Repack().IsMatch(tags),
            Variant: Variant(tags));
    }

    private static NyaaEpisode Episode(string number, string label) =>
        int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var regular) && regular > 0
            ? new RegularEpisode(regular, label)
            : new SpecialEpisode(decimal.Parse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture), label);

    private static string Variant(string tags) =>
        Whitespace().Replace(PerEpisodeToken().Replace(tags, " "), " ").Trim();

    // A bare NN needs the " - " separator ("Season 2 - 03" is episode 3); SxxEyy is unambiguous without it.
    // The episode must end at a boundary so "2nd Season" or "01-12" never reads as an episode.
    [GeneratedRegex(
        @"^\[(?<group>[^\]]+)\]\s*(?:-\s+)?(?<title>.+?)(?:\s+(?:-\s+)?(?<label>[Ss]\d{1,3}[Ee](?<episode>\d{1,4}(?:\.\d+)?))|\s+-\s+(?<label>(?<episode>\d{1,4}(?:\.\d+)?)))(?:v(?<version>\d+))?(?=[\s\[(]|$)(?<tags>.*)$")]
    private static partial Regex Release();

    [GeneratedRegex(@"\bBatch\b|\(\d+\s*[-~]\s*\d+\)|\s-\s\d+\s*[-~]\s*\d+(?=[\s\[(]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex BatchMarker();

    [GeneratedRegex(@"\bREPACK\d*\b", RegexOptions.IgnoreCase)]
    private static partial Regex Repack();

    // CRC, an (SxxEyy) restating the episode, REPACK, and the file extension.
    [GeneratedRegex(@"\[[0-9A-Fa-f]{8}\]|\(?\b[Ss]\d{1,3}[Ee]\d{1,4}\b\)?|\bREPACK\d*\b|\.mkv$", RegexOptions.IgnoreCase)]
    private static partial Regex PerEpisodeToken();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
