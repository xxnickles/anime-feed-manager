using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IRemoveMovieSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class RemoveMovieSubscription : IRemoveMovieSubscription
{
    private readonly ITableClientFactory<MoviesSubscriptionStorage> _clientFactory;

    public RemoveMovieSubscription(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => Delete(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Delete(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        return TableUtils.TryExecute(() => client.DeleteEntityAsync(userId, series, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}