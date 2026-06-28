using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Seasons.Storage;

public static class CosmosLibrarySeasonsIndex
{
    private static readonly ActivitySource Source = new(Telemetry.LibraryImportSource);

    public static LibrarySeasonsIndexLoader LibrarySeasonsIndexLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => Load(container, cancellationToken))
            .Map(read => read.Value);

    public static LibrarySeasonsIndexUpserter LibrarySeasonsIndexUpserterHandler(this ICosmosContainerFactory factory) =>
        (entry, kind, cancellationToken) => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => LoadMergeUpsert(container, entry, kind, cancellationToken));

    // Built directly over the index container (reusing the private Load helper, not the
    // loader delegate) and projects to the latest season — an empty index is a NotFound,
    // keeping absence in the error channel.
    public static LatestSeasonResolver LatestSeasonResolverHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => Load(container, cancellationToken))
            .Bind(read => ResolveLatest(read.Value));

    // async (not a sync Task-returning method) so `using var activity` stays alive across the
    // awaited chain — disposing it on return would stop the span before the Tap tags it, and
    // SetTag on a stopped activity is a silent no-op. The read + upsert RU is captured here and
    // tagged on this span (we don't surface the cost up the abstraction — keeps the seam clean
    // and lets the trace attribute spend to this specific operation).
    private static async Task<Result<Unit>> LoadMergeUpsert(
        Container container,
        SeasonEntry entry,
        SeasonImportKind kind,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Library.Import.SeasonsIndex");
        return await Load(container, cancellationToken)
            .Bind(read => Upsert(container, read.Value with { Seasons = Merge(read.Value.Seasons, entry, kind) },
                    cancellationToken)
                .Tap(writeCost => activity?.SetTag(
                    "library.import.seasons_index.cost.ru",
                    Math.Round(read.RequestCharge + writeCost.RuUsed, 2)))
                .Map(_ => new Unit()));
    }

    private static async Task<Result<CosmosResult<LibrarySeasonsIndex>>> Load(
        Container container,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(SystemDocument.SystemPartitionKey);
        try
        {
            var response = await container.ReadItemAsync<LibrarySeasonsIndex>(
                LibrarySeasonsIndex.DocumentId,
                partitionKey,
                cancellationToken: cancellationToken);
            return new CosmosResult<LibrarySeasonsIndex>(response.Resource, response.RequestCharge);
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new CosmosResult<LibrarySeasonsIndex>(
                new LibrarySeasonsIndex { Id = LibrarySeasonsIndex.DocumentId }, e.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, LibrarySeasonsIndex.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<CosmosOperationCost>> Upsert(
        Container container,
        LibrarySeasonsIndex index,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(index.PartitionKey);
        try
        {
            var response = await container.UpsertItemAsync(
                index,
                partitionKey,
                cancellationToken: cancellationToken);
            return new CosmosOperationCost(response.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, index.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Prefer the explicitly-marked airing season; fall back to the calendar-latest
    // (libraries built only from specific imports never set the marker).
    internal static Result<SeriesSeason> ResolveLatest(LibrarySeasonsIndex index) =>
        index.Seasons.IsDefaultOrEmpty
            ? NotFoundError.Create("No seasons have been imported into the library yet.")
            : index.Seasons.FirstOrDefault(entry => entry.IsCurrent)?.SeriesSeason
              ?? index.Seasons.Max(entry => entry.SeriesSeason)!; // guarded non-empty above

    // Single-current invariant: a current-season import marks its entry and clears the
    // flag on every other season; a specific import preserves the existing entry's marker
    // (re-importing a back-catalog season never demotes the airing one).
    internal static ImmutableArray<SeasonEntry> Merge(
        ImmutableArray<SeasonEntry> existing,
        SeasonEntry incoming,
        SeasonImportKind kind)
    {
        if (kind is SeasonImportKind.Current)
        {
            var others = existing
                .Where(e => e.SeriesSeason != incoming.SeriesSeason)
                .Select(e => e with { IsCurrent = false });
            return [.. others, incoming with { IsCurrent = true }];
        }

        var match = existing.FirstOrDefault(e => e.SeriesSeason == incoming.SeriesSeason);
        return match is null
            ? existing.Add(incoming with { IsCurrent = false })
            : existing.Replace(match, incoming with { IsCurrent = match.IsCurrent });
    }
}
