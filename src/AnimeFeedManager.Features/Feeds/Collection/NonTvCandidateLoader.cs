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
/// Non-TV (movie/OVA/ONA/special) candidates for Nyaa matching, across every imported season,
/// excluding anything present in the currently-airing TV index. Loops <c>LibrarySeasonsIndex</c>
/// and reads each season's partition individually via <see cref="SeriesBySeasonLoader"/> — cost
/// scales with season count, not total series count.
/// </summary>
public delegate Task<Result<ImmutableArray<SeriesTitleProjection>>> NonTvCandidateLoader(CancellationToken cancellationToken);

public static class NonTvCandidates
{
    public static NonTvCandidateLoader NonTvCandidateLoaderHandler(this ICosmosContainerFactory factory)
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
                .Where(series => IsNonTvCandidate(series, airingMalIds))
                .Select(series => new SeriesTitleProjection(series.MalId, series.AllTitles))
                .ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<SeriesTitleProjection>>())
            .Map(bulk => bulk.Value);
    }

    // Only TvSeries carries a broadcast clock (Series.Schedule); every other variant
    // (Movie/OVA/ONA/TvSpecial/Special) is Nyaa-observation-only.
    internal static bool IsNonTvCandidate(Series series, FrozenSet<int> airingMalIds) =>
        series is not TvSeries && !airingMalIds.Contains(series.MalId);
}
