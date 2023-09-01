using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IAddTvSubscription
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class AddTvTvSubscription : IAddTvSubscription
{
    private readonly ITableClientFactory<SubscriptionStorage> _clientFactory;

    public AddTvTvSubscription(ITableClientFactory<SubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, CancellationToken token)
    {
        return _clientFactory.GetClient().BindAsync(client => Persist(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        var storage = new SubscriptionStorage
        {
            PartitionKey = userId,
            RowKey = series,
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}