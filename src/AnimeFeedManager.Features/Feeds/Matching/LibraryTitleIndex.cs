using System.Collections.Frozen;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Feeds.Matching;

/// <summary>
/// Normalized-title -> series id lookup, built once per collection run from the whole library
/// (every series, any season/status — sequels, BD releases, and movies can land regardless of
/// current season). Titles that normalize to nothing meaningful (e.g. Japanese-script-only
/// entries — Nyaa's English-translated category means raw release titles are romanized/English)
/// are skipped rather than indexed, to avoid unrelated series colliding on a near-empty key.
/// </summary>
internal sealed class LibraryTitleIndex
{
    private readonly FrozenDictionary<string, int> _bySeriesTitle;

    private LibraryTitleIndex(FrozenDictionary<string, int> bySeriesTitle) => _bySeriesTitle = bySeriesTitle;

    public static LibraryTitleIndex Build(IEnumerable<Series> library)
    {
        var map = new Dictionary<string, int>();
        AddEntries(map, library.Select(series => (series.MalId, series.AllTitles)));
        return new LibraryTitleIndex(map.ToFrozenDictionary());
    }

    /// <summary>
    /// Same as <see cref="Build(IEnumerable{Series})"/>, plus a set of lightweight title
    /// projections (e.g. long-running series pulled in from outside the current season) —
    /// for callers that don't have (or don't want to pay for) the full <see cref="Series"/>.
    /// </summary>
    public static LibraryTitleIndex Build(IEnumerable<Series> library, IEnumerable<SeriesTitleProjection> additional)
    {
        var map = new Dictionary<string, int>();
        AddEntries(map, library.Select(series => (series.MalId, series.AllTitles)));
        AddEntries(map, additional.Select(projection => (projection.MalId, projection.AllTitles)));
        return new LibraryTitleIndex(map.ToFrozenDictionary());
    }

    private static void AddEntries(Dictionary<string, int> map, IEnumerable<(int MalId, string[] AllTitles)> entries)
    {
        foreach (var (malId, titles) in entries)
        foreach (var title in titles)
        {
            var normalized = TitleNormalizer.Normalize(title);
            if (normalized.Length < 2) continue;
            map.TryAdd(normalized, malId);
        }
    }

    public bool TryMatch(string cleanTitle, out int seriesId) =>
        _bySeriesTitle.TryGetValue(TitleNormalizer.Normalize(cleanTitle), out seriesId);
}
