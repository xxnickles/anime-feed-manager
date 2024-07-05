using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IRemoveAllMoviesSubscriptions
{
    Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token);
}

public class RemoveAllMoviesSubscriptions : IRemoveAllMoviesSubscriptions
{
    private readonly ITableClientFactory<MoviesSubscriptionStorage> _clientFactory;

    public RemoveAllMoviesSubscriptions(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
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
                client.QueryAsync<MoviesSubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token))
            .BindAsync(items => RemoveAll(client, items, token));
    }

    private static Task<Either<DomainError, ProcessResult>> RemoveAll(TableClient client,
        ImmutableList<MoviesSubscriptionStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty) return Task.FromResult(Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.MoviesSubscriptions)));

        return TableUtils.BatchDelete(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count,ProcessScope.MoviesSubscriptions));
    }
}