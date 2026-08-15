using System.Diagnostics;
using AnimeFeedManager.Features.Library.Airing.Storage;
using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Mappers;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Library.Airing;

/// <summary>Refreshes the currently-airing-TV index from Jikan. Plain callable routine — used by both a daily <see cref="CronJob"/> and the <c>SeasonImported</c> event handler.</summary>
public sealed class AiringSeriesIndexRefreshJob(
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    ILogger<AiringSeriesIndexRefreshJob> logger)
{
    private readonly AiringSeriesIndexReplacer _replaceIndex = cosmosFactory.AiringSeriesIndexReplacerHandler();

    public async Task Run(CancellationToken cancellationToken)
    {
        var pages = new List<Result<ImmutableArray<AiringSeriesEntry>>>();
        await foreach (var pageResult in jikan.GetCurrentlyAiringTv(cancellationToken).WithCancellation(cancellationToken))
            pages.Add(pageResult.Bind(page => MapEntries(page.Items)));

        // A partial-page failure still fails the whole refresh (not a partial write) — an
        // incomplete airing index would silently miss whatever series fell in the failed pages,
        // worse than leaving the previous index untouched until the next run.
        await pages
            .Flatten(entryLists => entryLists.SelectMany(entries => entries).ToImmutableArray())
            .Bind(bulk => bulk switch
            {
                CompletedBulkResult<ImmutableArray<AiringSeriesEntry>> completed => Result<ImmutableArray<AiringSeriesEntry>>.Success(completed.Value),
                PartialSuccessBulkResult<ImmutableArray<AiringSeriesEntry>> partial => new AggregatedError(
                    "Some pages failed while refreshing the currently-airing TV index", partial.Errors),
                _ => throw new UnreachableException()
            })
            .Bind(entries => _replaceIndex(entries, cancellationToken))
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to refresh the currently-airing TV index"))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);
    }

    // A fully-unparseable page fails outright; individual bad items within a good page are
    // dropped and logged, not fatal.
    private Result<ImmutableArray<AiringSeriesEntry>> MapEntries(ImmutableArray<JikanAnime> items) =>
        items.Select(ToEntry)
            .Flatten(entries => entries.ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<AiringSeriesEntry>>())
            .Map(bulk => bulk.Value);

    private static Result<AiringSeriesEntry> ToEntry(JikanAnime anime) =>
        ParseSeason(anime)
            .Map(season => new AiringSeriesEntry(
                anime.MalId, season, JikanSeriesMapper.BuildAllTitles(JikanSeriesMapper.BuildTitles(anime.Titles))));

    // A type=tv result should always carry season/year; a miss is data-quality noise, not expected.
    private static Result<SeriesSeason> ParseSeason(JikanAnime anime) =>
        anime is { Season: { } season, Year: { } year }
            ? (season, year).ParseAsSeriesSeason()
            : Warning.Create($"Jikan anime {anime.MalId} has no season/year (expected for type=tv)");
}
