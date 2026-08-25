using System.Collections.Frozen;
using AnimeFeedManager.Features.Library.Airing;
using AnimeFeedManager.Features.Library.Airing.Storage;
using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Candidates for Nyaa matching outside the active airing window — every series, of any type,
/// not present in the currently-airing TV index. Movie/OVA/ONA/special series are always in
/// scope (that index is TV-only); TV series rejoin once they drop out of it (finished, or never
/// classified as airing). Loops <c>LibrarySeasonsIndex</c> and reads each season's partition
/// individually via <see cref="SeriesBySeasonLoader"/> — cost scales with season count, not
/// total series count.
/// </summary>
public delegate Task<Result<ImmutableArray<SeriesTitleProjection>>> NonAiringCandidateLoader(CancellationToken cancellationToken);

public static class NonAiringCandidates
{
    public static NonAiringCandidateLoader NonAiringCandidateLoaderHandler(this ICosmosContainerFactory factory)
    {
        var loadSeasons = factory.LibrarySeasonsIndexLoaderHandler();
        var loadSeries = factory.SeriesBySeasonLoaderHandler();
        var loadAiringIndex = factory.AiringSeriesIndexLoaderHandler();

        return cancellationToken => Load(loadSeasons, loadSeries, loadAiringIndex, cancellationToken);
    }

    private static Task<Result<ImmutableArray<SeriesTitleProjection>>> Load(
        LibrarySeasonsIndexLoader loadSeasons,
        SeriesBySeasonLoader loadSeries,
        AiringSeriesIndexLoader loadAiringIndex,
        CancellationToken cancellationToken) =>
        loadSeasons(cancellationToken)
            .Bind(seasonsIndex => loadAiringIndex(cancellationToken)
                .Bind(airingIndex => LoadCandidates(seasonsIndex.Seasons, airingIndex.Entries, loadSeries, cancellationToken)));

    // A season-read failure fails the whole pass (not a partial candidate set) — same
    // all-or-nothing shape as the airing-index refresh, for the same reason: a silently
    // incomplete candidate set would miss series without any signal that coverage narrowed.
    private static async Task<Result<ImmutableArray<SeriesTitleProjection>>> LoadCandidates(
        ImmutableArray<SeasonEntry> seasons,
        ImmutableArray<AiringSeriesEntry> airingEntries,
        SeriesBySeasonLoader loadSeries,
        CancellationToken cancellationToken)
    {
        var airingMalIds = airingEntries.Select(entry => entry.MalId).ToFrozenSet();

        var perSeason = new List<Result<ImmutableArray<Series>>>(seasons.Length);
        foreach (var season in seasons)
            perSeason.Add(await loadSeries(season.SeriesSeason, cancellationToken));

        return perSeason
            .Flatten(seriesLists => seriesLists
                .SelectMany(series => series)
                .Where(series => IsNonAiringCandidate(series, airingMalIds))
                .Select(series => new SeriesTitleProjection(series.MalId, series.AllTitles))
                .ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<SeriesTitleProjection>>())
            .Map(bulk => bulk.Value);
    }

    // The airing index is TV-only (sourced from Jikan's currently-airing-TV endpoint), so a
    // non-TV series' MalId can never appear in it — this reduces to "not currently airing,"
    // regardless of type. That's what lets a TV series rejoin once it finishes airing.
    internal static bool IsNonAiringCandidate(Series series, FrozenSet<int> airingMalIds) =>
        !airingMalIds.Contains(series.MalId);
}
