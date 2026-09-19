namespace AnimeFeedManager.Web.BlazorComponents.Email.Templates;

/// <summary>
/// Episode numbers reach the email as the feed renders them: digits with an optional
/// version suffix ("07", "1150", "07v2"). Ordinal comparison breaks across digit
/// widths, so ordering goes through a parsed key.
/// </summary>
public static class EpisodeOrder
{
    private const int UnversionedRelease = 1;

    /// <summary>
    /// Highest episode number first; a version bump outranks the original release.
    /// Values without a leading number sort last, keeping their source order.
    /// </summary>
    public static IEnumerable<EpisodeInfo> NewestFirst(this IEnumerable<EpisodeInfo> episodes) =>
        episodes.OrderByDescending(episode => SortKey(episode.EpisodeNumber));

    private static (int Number, int Version) SortKey(string episodeNumber)
    {
        var value = episodeNumber.AsSpan().Trim();

        var digits = 0;
        while (digits < value.Length && char.IsAsciiDigit(value[digits]))
            digits++;

        if (digits == 0 || !int.TryParse(value[..digits], out var number))
            return (-1, -1);

        var suffix = value[digits..];

        return suffix.Length > 1 && suffix[0] is 'v' or 'V' && int.TryParse(suffix[1..], out var version)
            ? (number, version)
            : (number, UnversionedRelease);
    }
}
