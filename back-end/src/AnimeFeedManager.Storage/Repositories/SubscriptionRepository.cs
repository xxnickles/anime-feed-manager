using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using Azure;
using Azure.Data.Tables;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly TableClient _tableClient;

    public SubscriptionRepository(ITableClientFactory<SubscriptionStorage> tableClientFactory) => _tableClient = tableClientFactory.GetClient();

    public Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> Get(Email userEmail)
    {
        var user = OptionUtils.UnpackOption(userEmail.Value, string.Empty);
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == user));
    }

    public Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> GetAll() =>
        TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<SubscriptionStorage>());


    public async Task<Either<DomainError, Unit>> Merge(SubscriptionStorage subscription)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpdateEntityAsync(subscription, ETag.All));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, Unit>> Delete(SubscriptionStorage subscription)
    {
        var result = await TableUtils.TryExecute(() =>
            _tableClient.DeleteEntityAsync(subscription.PartitionKey, subscription.RowKey));
        return result.Map(_ => unit);
    }
}