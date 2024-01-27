using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface ICopyMoviesSubscriptions
{
    Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token);
}

public class CopyMoviesSubscriptions : ICopyMoviesSubscriptions
{
    private readonly ITableClientFactory<MoviesSubscriptionStorage> _clientFactory;

    public CopyMoviesSubscriptions(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
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
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<MoviesSubscriptionStorage>(s => s.PartitionKey == source, cancellationToken: token))
            .BindAsync(items => StoreAll(client, items.ConvertAll(i => ReMap(i, target)), token));
    }

    private static Task<Either<DomainError, ProcessResult>> StoreAll(TableClient client,
        ImmutableList<MoviesSubscriptionStorage> moviesSubscriptions,
        CancellationToken token)
    {
        if (moviesSubscriptions.IsEmpty)
            return Task.FromResult(
                Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.MoviesSubscriptions)));

        return TableUtils.BatchAdd(client, moviesSubscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) moviesSubscriptions.Count, ProcessScope.MoviesSubscriptions));
    }

    private static MoviesSubscriptionStorage ReMap(MoviesSubscriptionStorage moviesSubscriptionsStorage, UserId target)
    {
        moviesSubscriptionsStorage.PartitionKey = target;
        return moviesSubscriptionsStorage;
    }
}