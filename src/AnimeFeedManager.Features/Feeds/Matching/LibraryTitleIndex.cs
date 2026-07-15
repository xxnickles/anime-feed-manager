using System.Collections.Frozen;
using AnimeFeedManager.Features.Library.Entities;
using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

namespace AnimeFeedManager.Features.Feeds.Matching;

/// <summary>
/// Title -> series id lookup, built once per collection run from the whole library (every
/// series, any season/status — sequels, BD releases, and movies can land regardless of current
/// season). Matching is exact-normalized first, falling back to fuzzy word-set scoring: fansub
/// groups routinely add/drop whole words (English glosses, "Movie" suffixes, year
/// disambiguators) that defeat exact matching but still score high on token-set overlap. Titles
/// that normalize to nothing meaningful (e.g. Japanese-script-only entries — Nyaa's
/// English-translated category means raw release titles are romanized/English) are skipped
/// entirely, to avoid unrelated series colliding on a near-empty key.
/// </summary>
internal sealed class LibraryTitleIndex
{
    // Starting point, not measured — tune from observed false-positive/negative behavior once
    // this runs against real feed traffic.
    private const int FuzzyMatchThreshold = 85;

    private readonly FrozenDictionary<string, int> _bySeriesTitle;
    private readonly string[] _fuzzyTitles;
    private readonly int[] _fuzzySeriesIds;
    private readonly TokenSetScorer _fuzzyScorer = new();

    private LibraryTitleIndex(FrozenDictionary<string, int> bySeriesTitle, string[] fuzzyTitles, int[] fuzzySeriesIds)
    {
        _bySeriesTitle = bySeriesTitle;
        _fuzzyTitles = fuzzyTitles;
        _fuzzySeriesIds = fuzzySeriesIds;
    }

    public static LibraryTitleIndex Build(IEnumerable<Series> library) =>
        Build(library.Select(series => (series.MalId, series.AllTitles)));

    /// <summary>
    /// Same as <see cref="Build(IEnumerable{Series})"/>, plus a set of lightweight title
    /// projections (e.g. long-running series pulled in from outside the current season) —
    /// for callers that don't have (or don't want to pay for) the full <see cref="Series"/>.
    /// </summary>
    public static LibraryTitleIndex Build(IEnumerable<Series> library, IEnumerable<SeriesTitleProjection> additional) =>
        Build(library.Select(series => (series.MalId, series.AllTitles))
            .Concat(additional.Select(projection => (projection.MalId, projection.AllTitles))));

    private static LibraryTitleIndex Build(IEnumerable<(int MalId, string[] AllTitles)> entries)
    {
        var bySeriesTitle = new Dictionary<string, int>();
        var fuzzyTitles = new List<string>();
        var fuzzySeriesIds = new List<int>();

        foreach (var (malId, titles) in entries)
        foreach (var title in titles)
        {
            var normalized = TitleNormalizer.Normalize(title);
            if (normalized.Length < 2) continue;

            bySeriesTitle.TryAdd(normalized, malId);
            fuzzyTitles.Add(title);
            fuzzySeriesIds.Add(malId);
        }

        return new LibraryTitleIndex(bySeriesTitle.ToFrozenDictionary(), [..fuzzyTitles], [..fuzzySeriesIds]);
    }

    public bool TryMatch(string cleanTitle, out int seriesId)
    {
        if (_bySeriesTitle.TryGetValue(TitleNormalizer.Normalize(cleanTitle), out seriesId))
            return true;

        return TryFuzzyMatch(cleanTitle, out seriesId);
    }

    private bool TryFuzzyMatch(string cleanTitle, out int seriesId)
    {
        seriesId = 0;
        if (_fuzzyTitles.Length == 0) return false;

        var best = Process.ExtractOne(cleanTitle, _fuzzyTitles, processor: null, scorer: _fuzzyScorer, cutoff: FuzzyMatchThreshold);
        if (best is null) return false;

        seriesId = _fuzzySeriesIds[best.Index];
        return true;
    }
}
