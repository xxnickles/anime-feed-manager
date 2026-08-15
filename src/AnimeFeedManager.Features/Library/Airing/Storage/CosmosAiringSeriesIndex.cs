using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Airing.Storage;

public static class CosmosAiringSeriesIndex
{
    private static readonly ActivitySource Source = new(Telemetry.LibraryAiringSource);

    public static AiringSeriesIndexLoader AiringSeriesIndexLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<AiringSeriesIndex>()
            .Bind(container => Load(container, cancellationToken))
            .Map(read => read.Value);

    public static AiringSeriesIndexReplacer AiringSeriesIndexReplacerHandler(this ICosmosContainerFactory factory) =>
        (entries, cancellationToken) => factory.GetContainer<AiringSeriesIndex>()
            .Bind(container => Replace(container, entries, cancellationToken));

    private static async Task<Result<Unit>> Replace(
        Container container, ImmutableArray<AiringSeriesEntry> entries, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Library.Airing.IndexReplace");
        activity?.SetTag("library.airing.entry_count", entries.Length);

        var index = new AiringSeriesIndex { Id = AiringSeriesIndex.DocumentId, Entries = entries };
        var partitionKey = new PartitionKey(index.PartitionKey);
        try
        {
            var response = await container.UpsertItemAsync(index, partitionKey, cancellationToken: cancellationToken);
            activity?.SetTag("library.airing.cost.ru", Math.Round(response.RequestCharge, 2));
            return new Unit();
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

    private static async Task<Result<CosmosResult<AiringSeriesIndex>>> Load(
        Container container, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Library.Airing.IndexRead");
        var partitionKey = new PartitionKey(SystemDocument.SystemPartitionKey);
        try
        {
            var response = await container.ReadItemAsync<AiringSeriesIndex>(
                AiringSeriesIndex.DocumentId, partitionKey, cancellationToken: cancellationToken);
            activity?.SetTag("library.airing.cost.ru", Math.Round(response.RequestCharge, 2));
            activity?.SetTag("library.airing.entry_count", response.Resource.Entries.Length);
            return new CosmosResult<AiringSeriesIndex>(response.Resource, response.RequestCharge);
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new CosmosResult<AiringSeriesIndex>(
                new AiringSeriesIndex { Id = AiringSeriesIndex.DocumentId }, e.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, AiringSeriesIndex.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }
}
