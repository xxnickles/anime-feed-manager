using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IOvasSubscriptionStore
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, RowKey series, DateTime notificationDate,
        CancellationToken token);

    public Task<Either<DomainError, Unit>> Upsert(OvasSubscriptionStorage entity, CancellationToken token);
}

public sealed class OvasSubscriptionStoreStore(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
    : IOvasSubscriptionStore
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, RowKey series, DateTime notificationDate,
        CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client => Persist(client, userId, series, notificationDate, token));
    }

    public Task<Either<DomainError, Unit>> Upsert(OvasSubscriptionStorage entity, CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client =>
            TableUtils.TryExecute(() => client.UpsertEntityAsync(entity, cancellationToken: token))
                .MapAsync(_ => unit));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, RowKey series,
        DateTime notificationDate,
        CancellationToken token)
    {
        var storage = new OvasSubscriptionStorage
        {
            PartitionKey = userId,
            RowKey = series,
            DateToNotify = notificationDate
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}