using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveAllTvSubscriptions
{
    Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token);
}

public class RemoveAllTvSubscriptions : IRemoveAllTvSubscriptions
{
    private readonly ITableClientFactory<SubscriptionStorage> _clientFactory;

    public RemoveAllTvSubscriptions(ITableClientFactory<SubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => RemoveAllSubscription(client, userId, token));
    }

    private Task<Either<DomainError, ProcessResult>> RemoveAllSubscription(TableClient client, UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token))
            .BindAsync(items => RemoveAll(client, items, token));
    }

    private Task<Either<DomainError, ProcessResult>> RemoveAll(TableClient client,
        ImmutableList<SubscriptionStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty) return Task.FromResult(Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.TvSubscriptions)));

        return TableUtils.BatchDelete(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count, ProcessScope.TvSubscriptions));
    }
}