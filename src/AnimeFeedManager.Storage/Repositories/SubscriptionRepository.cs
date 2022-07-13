using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly TableClient _tableClient;

    public SubscriptionRepository(ITableClientFactory<SubscriptionStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }
    

    public Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> Get(Email userEmail)
    {
        var user = OptionUtils.UnpackOption(userEmail.Value, string.Empty);
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == user), nameof(SubscriptionStorage));
    }

    public Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> GetAll() =>
        TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<SubscriptionStorage>(), nameof(SubscriptionStorage));


    public async Task<Either<DomainError, Unit>> Merge(SubscriptionStorage subscription)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(subscription), nameof(SubscriptionStorage));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, Unit>> Delete(SubscriptionStorage subscription)
    {
        var result = await TableUtils.TryExecute(() =>
            _tableClient.DeleteEntityAsync(subscription.PartitionKey, subscription.RowKey), nameof(SubscriptionStorage));
        return result.Map(_ => unit);
    }
}