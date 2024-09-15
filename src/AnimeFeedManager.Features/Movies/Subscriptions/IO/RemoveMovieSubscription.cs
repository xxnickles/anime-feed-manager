using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IRemoveMovieSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, RowKey series, CancellationToken token);
}

public sealed class RemoveMovieSubscription(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
    : IRemoveMovieSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, RowKey series,
        CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => Delete(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Delete(TableClient client, UserId userId, RowKey series,
        CancellationToken token)
    {
        return TableUtils.TryExecute(() => client.DeleteEntityAsync(userId, series, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}