using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IGetMovieSubscriptions
{
    Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token);
    
    Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetCompleteSubscriptions(UserId userId, CancellationToken token);
    
    Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetSubscriptionForMovie(RowKey rowKey,  CancellationToken token);
}

public sealed class GetMovieSubscriptions(ITableClientFactory<MoviesSubscriptionStorage> clientFactory) : IGetMovieSubscriptions
{
    public Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<MoviesSubscriptionStorage>(storage => storage.PartitionKey == userId,
                    cancellationToken: token)))
            .MapAsync(subscriptions => subscriptions.ConvertAll(s => s.RowKey ?? string.Empty));
    }

    public Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetCompleteSubscriptions(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<MoviesSubscriptionStorage>(storage => storage.PartitionKey == userId,
                    cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetSubscriptionForMovie(RowKey rowKey, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<MoviesSubscriptionStorage>(storage => storage.RowKey == rowKey && storage.Processed,
                    cancellationToken: token)));
    }
}