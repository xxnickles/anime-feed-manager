using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IGetOvasSubscriptions
{
    Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token);

    Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetCompleteSubscriptions(UserId userId,
        CancellationToken token);

    Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetSubscriptionForOva(RowKey rowKey,
        CancellationToken token);
}

public sealed class GetOvasSubscriptions(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
    : IGetOvasSubscriptions
{
    public Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(storage => storage.PartitionKey == userId,
                    cancellationToken: token)))
            .MapAsync(subscriptions =>
                subscriptions.ConvertAll(s => s.RowKey?.RestoreForbiddenRowKeyParameters() ?? string.Empty));
    }

    public Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetCompleteSubscriptions(UserId userId,
        CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(storage => storage.PartitionKey == userId,
                    cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(AddReplacedCharacters));
    }

    public Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetSubscriptionForOva(RowKey rowKey,
        CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(storage => storage.RowKey == rowKey && storage.Processed,
                    cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(AddReplacedCharacters));
    }

    private static OvasSubscriptionStorage AddReplacedCharacters(OvasSubscriptionStorage storage)
    {
        storage.RowKey = storage.RowKey?.RestoreForbiddenRowKeyParameters() ?? string.Empty;
        return storage;
    }
}