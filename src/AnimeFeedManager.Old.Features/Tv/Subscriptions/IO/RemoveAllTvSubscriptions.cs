using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

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

    private static Task<Either<DomainError, ProcessResult>> RemoveAllSubscription(TableClient client, UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token))
            .BindAsync(items => RemoveAll(client, items, token));
    }

    private static Task<Either<DomainError, ProcessResult>> RemoveAll(TableClient client,
        ImmutableList<SubscriptionStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty) return Task.FromResult(Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.TvSubscriptions)));

        return TableUtils.BatchDelete(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count, ProcessScope.TvSubscriptions));
    }
}