using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly TableClient _tableClient;

    private class SubscriptionContainer : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public SubscriptionRepository(ITableClientFactory<SubscriptionStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }


    public Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> Get(Email userEmail)
    {
        var user = userEmail.Value.UnpackOption(string.Empty);
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == user), nameof(SubscriptionStorage));
    }

    public Task<Either<DomainError, ImmutableList<string>>> GetAllSubscribers()
    {
        return TableUtils.ExecuteQuery(
                () => _tableClient.QueryAsync<SubscriptionContainer>(select: new[]
                    {nameof(SubscriptionContainer.PartitionKey)}),
                nameof(SubscriptionStorage))
            .MapAsync(list => 
                list.Select(subs => subs.PartitionKey)
                    .Distinct()
                    .ToImmutableList());
    }


    public Task<Either<DomainError, Unit>> Merge(SubscriptionStorage subscription)
    {
        return TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(subscription), nameof(SubscriptionStorage))
            .MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> Delete(SubscriptionStorage subscription)
    {
        return TableUtils.TryExecute(() =>
                    _tableClient.DeleteEntityAsync(subscription.PartitionKey, subscription.RowKey),
                nameof(SubscriptionStorage))
            .MapAsync(_ => unit);
    }
}