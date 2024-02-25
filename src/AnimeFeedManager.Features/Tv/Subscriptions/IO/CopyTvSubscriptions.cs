using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface ICopyTvSubscriptions
{
    Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token);
}

public class CopyTvSubscriptions : ICopyTvSubscriptions
{
    private readonly ITableClientFactory<SubscriptionStorage> _clientFactory;

    public CopyTvSubscriptions(ITableClientFactory<SubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => CopySubscription(client, source, target, token));
    }


    private Task<Either<DomainError, ProcessResult>> CopySubscription(TableClient client, UserId source, UserId target,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == source, cancellationToken: token))
            .BindAsync(items => StoreAll(client, items.ConvertAll(i => ReMap(i, target)), token));
    }

    private static Task<Either<DomainError, ProcessResult>> StoreAll(TableClient client,
        ImmutableList<SubscriptionStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty)
            return Task.FromResult(
                Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.TvSubscriptions)));

        return TableUtils.BatchAdd(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count, ProcessScope.TvSubscriptions));
    }

    private static SubscriptionStorage ReMap(SubscriptionStorage subscriptionStorage, UserId target)
    {
        subscriptionStorage.PartitionKey = target;
        return subscriptionStorage;
    }
}