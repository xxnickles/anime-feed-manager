using System.Diagnostics;
using AnimeFeedManager.Features.Library.Airing.Storage;
using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Import.Jikan;
using AnimeFeedManager.Features.Library.Import.Jikan.Mappers;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Library.Airing;

/// <summary>
/// Refreshes the currently-airing-TV index from Jikan. A plain callable routine, not tied to a
/// single trigger — called by a daily <see cref="CronJob"/> and directly from <c>LibraryImport</c>'s
/// post-import step. Each run is a full replace: a series absent from this pull is, by
/// construction, no longer airing.
/// </summary>
public sealed class AiringSeriesIndexRefreshJob(
    IJikanClient jikan,
    ICosmosContainerFactory cosmosFactory,
    ILogger<AiringSeriesIndexRefreshJob> logger)
{
    private readonly AiringSeriesIndexReplacer _replaceIndex = cosmosFactory.AiringSeriesIndexReplacerHandler();

    public async Task Run(CancellationToken cancellationToken)
    {
        var accumulated = new List<Result<ImmutableArray<AiringSeriesEntry>>>();

        await foreach (var pageResult in jikan.GetCurrentlyAiringTv(cancellationToken).WithCancellation(cancellationToken))
        {
            var mapped = pageResult.Bind(page => MapEntries(page.Items));
            accumulated.Add(mapped);
            if (mapped.IsFailure) break;
        }

        await accumulated
            .Flatten(pages => pages.SelectMany(page => page).ToImmutableArray())
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

    // A page failing entirely (every item unparseable) is a genuine failure that propagates up
    // to the outer page-accumulation loop. Individual unparseable items within an otherwise-good
    // page are dropped, not fatal — mirrors the "one bad item shouldn't cost the run every other
    // entry" precedent elsewhere in this codebase (e.g. NyaaClient.ParseEntries), but composed
    // through Result/BulkResult throughout rather than collapsed to nullable partway.
    private Result<ImmutableArray<AiringSeriesEntry>> MapEntries(ImmutableArray<JikanAnime> items) =>
        items.Select(ToEntry)
            .Flatten(entries => entries.ToImmutableArray())
            .Tap(bulk => bulk.LogResults(logger, static (_, _) => { }))
            .Map(bulk => bulk.Value);

    private static Result<AiringSeriesEntry> ToEntry(JikanAnime anime) =>
        ParseSeason(anime)
            .Map(season => new AiringSeriesEntry(
                anime.MalId, season, JikanSeriesMapper.BuildAllTitles(JikanSeriesMapper.BuildTitles(anime.Titles))));

    // season/year are TV-only on Jikan and expected to be populated for a type=tv result; a miss
    // is unexpected data quality, not a normal case. ParseAsSeriesSeason additionally validates
    // both components are well-formed.
    private static Result<SeriesSeason> ParseSeason(JikanAnime anime) =>
        anime is { Season: { } season, Year: { } year }
            ? (season, year).ParseAsSeriesSeason()
            : Warning.Create($"Jikan anime {anime.MalId} has no season/year (expected for type=tv)");
}
