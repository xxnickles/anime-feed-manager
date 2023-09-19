using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IAddMovieSubscription
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, DateTime notificationDate,
        CancellationToken token);
}

public sealed class AddMovieSubscription : IAddMovieSubscription
{
    private readonly ITableClientFactory<MoviesSubscriptionStorage> _clientFactory;

    public AddMovieSubscription(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, DateTime notificationDate,
        CancellationToken token)
    {
        return _clientFactory.GetClient().BindAsync(client => Persist(client, userId, series, notificationDate, token));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, NoEmptyString series,
        DateTime notificationDate,
        CancellationToken token)
    {
        var storage = new MoviesSubscriptionStorage
        {
            PartitionKey = userId,
            RowKey = series,
            DateToNotify = notificationDate
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}