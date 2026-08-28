using System.Collections.Frozen;
using AnimeFeedManager.Features.Library.Entities;
using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

namespace AnimeFeedManager.Features.Feeds.Matching;

/// <summary>
/// The data a successful match guarantees — constructed once per indexed series, never
/// assembled piecemeal from separate lookups, so there's no "matched but missing title/season"
/// state to defend against downstream.
/// </summary>
internal sealed record MatchedSeries(int MalId, string Title, SeriesSeason Season);

/// <summary>
/// Title -> series lookup, built once per collection run from the whole library (every
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

    private readonly FrozenDictionary<string, MatchedSeries> _bySeriesTitle;
    private readonly string[] _fuzzyTitles;
    private readonly MatchedSeries[] _fuzzySeries;
    private readonly TokenSetScorer _fuzzyScorer = new();

    private LibraryTitleIndex(
        FrozenDictionary<string, MatchedSeries> bySeriesTitle,
        string[] fuzzyTitles,
        MatchedSeries[] fuzzySeries)
    {
        _bySeriesTitle = bySeriesTitle;
        _fuzzyTitles = fuzzyTitles;
        _fuzzySeries = fuzzySeries;
    }

    public static LibraryTitleIndex Build(IEnumerable<Series> library) =>
        Build(library.Select(series => (series.MalId, series.AllTitles, series.SeriesSeason)));

    /// <summary>
    /// Same as <see cref="Build(IEnumerable{Series})"/>, plus a set of lightweight title
    /// projections (e.g. long-running series pulled in from outside the current season) —
    /// for callers that don't have (or don't want to pay for) the full <see cref="Series"/>.
    /// </summary>
    public static LibraryTitleIndex Build(IEnumerable<Series> library, IEnumerable<SeriesTitleProjection> additional) =>
        Build(library.Select(series => (series.MalId, series.AllTitles, series.SeriesSeason))
            .Concat(additional.Select(projection => (projection.MalId, projection.AllTitles, projection.Season))));

    private static LibraryTitleIndex Build(IEnumerable<(int MalId, string[] AllTitles, SeriesSeason Season)> entries)
    {
        var bySeriesTitle = new Dictionary<string, MatchedSeries>();
        var fuzzyTitles = new List<string>();
        var fuzzySeries = new List<MatchedSeries>();

        foreach (var (malId, titles, season) in entries)
        {
            if (titles.Length == 0) continue; // nothing to match against or report as a title

            // AllTitles[0] is always the canonical/default title — see JikanSeriesMapper.BuildAllTitles.
            var matched = new MatchedSeries(malId, titles[0], season);

            foreach (var title in titles)
            {
                var normalized = TitleNormalizer.Normalize(title);
                if (normalized.Length < 2) continue;

                bySeriesTitle.TryAdd(normalized, matched);
                fuzzyTitles.Add(title);
                fuzzySeries.Add(matched);
            }
        }

        return new LibraryTitleIndex(bySeriesTitle.ToFrozenDictionary(), [..fuzzyTitles], [..fuzzySeries]);
    }

    /// <summary>The matched series' data, or <c>null</c> if nothing in the library matches.</summary>
    public MatchedSeries? TryMatch(string cleanTitle) =>
        _bySeriesTitle.TryGetValue(TitleNormalizer.Normalize(cleanTitle), out var exact) ? exact : TryFuzzyMatch(cleanTitle);

    private MatchedSeries? TryFuzzyMatch(string cleanTitle)
    {
        if (_fuzzyTitles.Length == 0) return null;

        var best = Process.ExtractOne(cleanTitle, _fuzzyTitles, processor: null, scorer: _fuzzyScorer, cutoff: FuzzyMatchThreshold);
        return best is null ? null : _fuzzySeries[best.Index];
    }
}
