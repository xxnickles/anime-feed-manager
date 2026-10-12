namespace AnimeFeedManager.Features.SystemEvents.Notifications;

public abstract record EpisodeSegment(int From, int To)
{
    public int Length => To - From + 1;
    public string Label => From == To ? $"{From:00}" : $"{From:00}–{To:00}";
}

public sealed record EpisodeRange(int From, int To) : EpisodeSegment(From, To);

public sealed record EpisodeGap(int From, int To) : EpisodeSegment(From, To);

/// <summary>Notified episodes collapsed into ordered runs and holes over 01..Latest; a re-release covers its episode number.</summary>
public sealed record EpisodeCoverage(
    int Count,
    int Latest,
    ImmutableArray<EpisodeSegment> Segments,
    ImmutableArray<string> Rereleases)
{
    public static readonly EpisodeCoverage Empty = new(0, 0, [], []);
}

public static class EpisodeSegments
{
    /// <summary>Episodes follow the scraper's <c>\d+(v\d+)?</c> format; anything else is skipped.</summary>
    public static EpisodeCoverage From(IEnumerable<string> episodes)
    {
        var parsed = episodes
            .Select(Parse)
            .OfType<ParsedEpisode>()
            .ToArray();

        var numbers = parsed.Select(p => p.Number).Distinct().Order().ToArray();
        if (numbers.Length == 0) return EpisodeCoverage.Empty;

        var rereleases = parsed
            .Where(p => p.IsRerelease)
            .OrderBy(p => p.Number)
            .ThenBy(p => p.Raw, StringComparer.Ordinal)
            .Select(p => p.Raw)
            .Distinct();

        return new EpisodeCoverage(numbers.Length, numbers[^1], [..Collapse(numbers)], [..rereleases]);
    }

    private sealed record ParsedEpisode(int Number, string Raw, bool IsRerelease);

    private static ParsedEpisode? Parse(string episode)
    {
        var version = episode.IndexOf('v');
        var digits = version < 0 ? episode : episode[..version];
        return int.TryParse(digits, out var number) && number >= 0
            ? new ParsedEpisode(number, episode, version >= 0)
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
