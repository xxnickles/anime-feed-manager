using System.Linq.Expressions;
using System.Net;
using AnimeFeedManager.Features.Subscriptions.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AnimeFeedManager.Features.Subscriptions.Storage;

public static class CosmosUserSubscriptionStorage
{
    public static UserSubscriptionUpserter CosmosUserSubscriptionUpserter(this ICosmosContainerFactory factory) =>
        (subscription, cancellationToken) => factory.GetContainer<UserSubscription>()
            .Bind(container => Upsert(container, subscription, cancellationToken));

    public static UserSubscriptionRemover CosmosUserSubscriptionRemover(this ICosmosContainerFactory factory) =>
        (userId, seriesId, cancellationToken) => factory.GetContainer<UserSubscription>()
            .Bind(container => Remove(container, userId, seriesId, cancellationToken));

    public static UserSubscriptionsLoader CosmosUserSubscriptionsLoader(this ICosmosContainerFactory factory) =>
        (userId, cancellationToken) => factory.GetContainer<UserSubscription>()
            .Bind(container => LoadSubscriptions(container, userId, cancellationToken));

    private static async Task<Result<Unit>> Upsert(
        Container container, UserSubscription subscription, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(subscription.UserId);
        try
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(
                stream, subscription, SubscriptionsJsonContext.Default.UserDocument, cancellationToken);
            stream.Position = 0;

            using var response = await container.UpsertItemStreamAsync(stream, partitionKey, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"User subscription upsert failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey, subscription.Id, container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, subscription.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<Unit>> Remove(
        Container container, string userId, int seriesId, CancellationToken cancellationToken)
    {
        var documentId = $"subscription:{seriesId}";
        var partitionKey = new PartitionKey(userId);
        try
        {
            using var response = await container.DeleteItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"User subscription delete failed with status {response.StatusCode} ({response.ErrorMessage})",
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

    private static async Task<Result<ImmutableArray<UserSubscription>>> LoadSubscriptions(
        Container container, string userId, CancellationToken cancellationToken)
    {
        var result = await container.Query(new UserSubscriptionsQuery(userId), cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    // SeriesId is unique to UserSubscription among the doc types sharing this partition (account).
    // IS_DEFINED is the indexable type-guard discriminator.
    private sealed record UserSubscriptionsQuery(string UserId)
        : CosmosQuerySpecification<UserSubscription>(new PartitionKey(UserId), SortBy: null, SortDir: null)
    {
        public override IEnumerable<Expression<Func<UserSubscription, bool>>> Predicates() =>
            [subscription => subscription.SeriesId.IsDefined()];
    }
}
