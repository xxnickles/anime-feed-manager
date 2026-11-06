using System.Globalization;

namespace AnimeFeedManager.Features.SystemEvents.Notifications;

public abstract record EpisodeSegment(int From, int To)
{
    public int Length => To - From + 1;
    public string Label => From == To ? $"{From:00}" : $"{From:00}–{To:00}";
}

public sealed record EpisodeRange(int From, int To) : EpisodeSegment(From, To);

public sealed record EpisodeGap(int From, int To) : EpisodeSegment(From, To);

/// <summary>Notified episodes collapsed into ordered runs and holes over 01..Latest; a re-release covers its episode number, specials sit outside the runs.</summary>
public sealed record EpisodeCoverage(
    int Count,
    int Latest,
    ImmutableArray<EpisodeSegment> Segments,
    ImmutableArray<string> Rereleases,
    ImmutableArray<string> Specials)
{
    public static readonly EpisodeCoverage Empty = new(0, 0, [], [], []);
}

public static class EpisodeSegments
{
    /// <summary>Reads <c>05</c>, <c>05v2</c> and specials like <c>12.5</c> or <c>12.5v2</c>; anything else is skipped.</summary>
    public static EpisodeCoverage From(IEnumerable<string> episodes)
    {
        var parsed = episodes.Select(Parse).OfType<ParsedEpisode>().ToArray();
        if (parsed.Length == 0) return EpisodeCoverage.Empty;

        var regular = parsed.OfType<RegularEpisode>().ToArray();

        var numbers = regular.Select(p => p.Number).Distinct().Order().ToArray();
        var rereleases = regular
            .Where(p => p.IsRerelease)
            .OrderBy(p => p.Number)
            .ThenBy(p => p.Raw, StringComparer.Ordinal)
            .Select(p => p.Raw)
            .Distinct();
        ImmutableArray<string> specials =
        [
            ..parsed.OfType<SpecialEpisode>()
                .OrderBy(p => p.Number)
                .ThenBy(p => p.Raw, StringComparer.Ordinal)
                .Select(p => p.Raw)
                .Distinct()
        ];

        return numbers.Length == 0
            ? EpisodeCoverage.Empty with {Specials = specials}
            : new EpisodeCoverage(numbers.Length, numbers[^1], [..Collapse(numbers)], [..rereleases], specials);
    }

    private abstract record ParsedEpisode(string Raw);

    private sealed record RegularEpisode(int Number, string Raw, bool IsRerelease) : ParsedEpisode(Raw);

    private sealed record SpecialEpisode(decimal Number, string Raw) : ParsedEpisode(Raw);

    private static ParsedEpisode? Parse(string episode)
    {
        var version = episode.IndexOf('v');
        var number = version < 0 ? episode : episode[..version];

        if (int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var regular))
            return new RegularEpisode(regular, episode, version >= 0);

        return number.Contains('.')
               && decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var special)
            ? new SpecialEpisode(special, episode)
            : null;
    }

    private static IEnumerable<EpisodeSegment> Collapse(int[] numbers)
    {
        if (numbers[0] > 1) yield return new EpisodeGap(1, numbers[0] - 1);

        var start = numbers[0];
        for (var i = 1; i <= numbers.Length; i++)
        {
            if (i < numbers.Length && numbers[i] == numbers[i - 1] + 1) continue;

            yield return new EpisodeRange(start, numbers[i - 1]);
            if (i == numbers.Length) yield break;

            yield return new EpisodeGap(numbers[i - 1] + 1, numbers[i] - 1);
            start = numbers[i];
        }
    }
}
