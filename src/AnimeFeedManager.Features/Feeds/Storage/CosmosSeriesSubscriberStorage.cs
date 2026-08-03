using System.Linq.Expressions;
using System.Net;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosSeriesSubscriberStorage
{
    public static SeriesSubscriberUpserter CosmosSeriesSubscriberUpserterHandler(this ICosmosContainerFactory factory) =>
        (subscriber, cancellationToken) => factory.GetContainer<SeriesSubscriber>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, subscriber, cancellationToken));

    public static SeriesSubscriberRemover CosmosSeriesSubscriberRemoverHandler(this ICosmosContainerFactory factory) =>
        (seriesId, userId, cancellationToken) => factory.GetContainer<SeriesSubscriber>()
            .Bind(container => Remove(container, seriesId, userId, cancellationToken));

    public static SeriesSubscribersLoader CosmosSeriesSubscribersLoaderHandler(this ICosmosContainerFactory factory) =>
        (seriesId, cancellationToken) => factory.GetContainer<SeriesSubscriber>()
            .Bind(container => LoadSubscribers(container, seriesId, cancellationToken));

    private static async Task<Result<Unit>> Remove(
        Container container, int seriesId, string userId, CancellationToken cancellationToken)
    {
        var documentId = $"subscriber:{seriesId}:{userId}";
        var partitionKey = new PartitionKey(seriesId.ToString());
        try
        {
            using var response = await container.DeleteItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"Series subscriber delete failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey, documentId, container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, documentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<ImmutableArray<SeriesSubscriber>>> LoadSubscribers(
        Container container, int seriesId, CancellationToken cancellationToken)
    {
        var result = await container.Query(new SeriesSubscribersQuery(seriesId), cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    // UserId is unique to SeriesSubscriber among the doc types sharing this partition (classification,
    // confirmation, flag, releases). IS_DEFINED is the indexable type-guard discriminator — a plain
    // `!= null` comparison against a missing property is unreliable across the Cosmos LINQ provider.
    private sealed record SeriesSubscribersQuery(int SeriesId)
        : CosmosQuerySpecification<SeriesSubscriber>(new PartitionKey(SeriesId.ToString()), SortBy: null, SortDir: null)
    {
        public override IEnumerable<Expression<Func<SeriesSubscriber, bool>>> Predicates() =>
            [subscriber => subscriber.UserId.IsDefined()];
    }
}
