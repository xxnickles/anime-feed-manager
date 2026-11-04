using AnimeFeedManager.Features.Scrapping.SubsPlease;
using Raffinert.FuzzySharp;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

/// <param name="Title">Library title, for display.</param>
/// <param name="Titles">Library title, English title and synonyms.</param>
public sealed record BindingCandidate(string SeriesId, string Title, string FeedTitle, ImmutableArray<string> Titles);

public sealed record ScheduleMatch(string ShowTitle, string SeriesId);

/// <summary>A show left unbound: candidates share its best score, or it shares one candidate's best score with other shows.</summary>
public sealed record ScheduleTie(string ShowTitle, ImmutableArray<TiedSeries> Series);

public sealed record TiedSeries(string SeriesId, string Title);

public sealed record ScheduleMatches(ImmutableArray<ScheduleMatch> Matches, ImmutableArray<ScheduleTie> Ties)
{
    public static readonly ScheduleMatches None = new([], []);
}

internal static class ScheduleMatching
{
    // WeightedRatio scores a title contained in a longer one at 90, which covers SubsPlease's
    // shortened sequel and subtitle titles; a previous season scores lower than the exact sequel.
    internal const double MinimumScore = 90.0;

    private sealed record Keys(TiedSeries Series, ImmutableArray<string> Titles);

    private sealed record Claim(string Show, TiedSeries Series, double Score);

    /// <summary>Each show takes its best-scoring candidate at or above <see cref="MinimumScore"/>; equal best scores bind nothing.</summary>
    internal static ScheduleMatches Match(IEnumerable<ScheduledShow> shows, IEnumerable<BindingCandidate> candidates)
    {
        var keys = candidates.Select(ToKeys).Where(key => !key.Titles.IsEmpty).ToImmutableArray();
        var best = shows
            .Select(show => BestClaims(show.Title, keys))
            .Where(claims => !claims.IsEmpty)
            .ToImmutableArray();

        var resolved = best
            .Where(claims => claims.Length == 1)
            .GroupBy(claims => claims[0].Series.SeriesId, claims => claims[0])
            .Select(Resolve)
            .ToImmutableArray();

        return new ScheduleMatches(
            [.. resolved.SelectMany(claim => claim.Matches)],
            [
                .. best
                    .Where(claims => claims.Length > 1)
                    .Select(claims => new ScheduleTie(claims[0].Show, [.. claims.Select(claim => claim.Series)])),
                .. resolved.SelectMany(claim => claim.Ties)
            ]);
    }

    private static Keys ToKeys(BindingCandidate candidate) =>
        new(new TiedSeries(candidate.SeriesId, candidate.Title),
        [
            .. candidate.Titles.Prepend(candidate.FeedTitle)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Select(ScheduleTitles.Normalize)
                .Distinct()
        ]);

    private static ImmutableArray<Claim> BestClaims(string show, ImmutableArray<Keys> keys)
    {
        var normalized = ScheduleTitles.Normalize(show);
        var scored = keys
            .Select(key => new Claim(show, key.Series, key.Titles.Max(title => Fuzz.WeightedRatio(normalized, title))))
            .Where(claim => claim.Score >= MinimumScore)
            .ToImmutableArray();

        return scored.IsEmpty ? [] : [.. scored.Where(claim => claim.Score == scored.Max(top => top.Score))];
    }

    private static (IEnumerable<ScheduleMatch> Matches, IEnumerable<ScheduleTie> Ties) Resolve(
        IGrouping<string, Claim> claims)
    {
        var top = claims.Max(claim => claim.Score);
        var winners = claims.Where(claim => claim.Score == top).ToImmutableArray();

        return winners.Length == 1
            ? ([new ScheduleMatch(winners[0].Show, claims.Key)], [])
            : ([], winners.Select(winner => new ScheduleTie(winner.Show, [winner.Series])));
    }
}
