using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class OvasSubscriptionRepository : IOvasSubscriptionRepository
{
    private readonly TableClient _tableClient;

    public OvasSubscriptionRepository(ITableClientFactory<OvasSubscriptionStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetTodaySubscriptions()
    {
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<OvasSubscriptionStorage>(
                s => s.DateToNotify >= DateTime.Today), nameof(OvasSubscriptionStorage));
    }

    public Task<Either<DomainError, Unit>> Complete(string subscriber, string title)
    {
        OvasSubscriptionStorage MarkCompleted(OvasSubscriptionStorage storage)
        {
            storage.Processed = true;
            return storage;
        }
        
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<OvasSubscriptionStorage>(s =>
                    s.PartitionKey == subscriber && s.RowKey == title), nameof(OvasSubscriptionStorage))
            .MapAsync(x => MarkCompleted(x.First()))
            
            .BindAsync(Merge);
    }

    public Task<Either<DomainError, Unit>> Merge(OvasSubscriptionStorage subscription)
    {
        return TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(subscription),
            nameof(OvaStorage)).MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> Delete(string subscriber, string title)
    {
        return TableUtils.TryExecute(() =>
                    _tableClient.DeleteEntityAsync(subscriber, title),
                nameof(SubscriptionStorage))
            .MapAsync(_ => unit);
    }
}