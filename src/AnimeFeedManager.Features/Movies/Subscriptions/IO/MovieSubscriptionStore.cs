using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public interface IMovieSubscriptionStore
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, RowKey series, DateTime notificationDate,
        CancellationToken token);
    
    public Task<Either<DomainError, Unit>> Upsert(MoviesSubscriptionStorage entity, CancellationToken token);
    
    public Task<Either<DomainError, Unit>> BulkUpdate(ImmutableList<MoviesSubscriptionStorage> entities, CancellationToken token);
}

public sealed class MovieSubscriptionStore(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
    : IMovieSubscriptionStore
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, RowKey series, DateTime notificationDate,
        CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client => Persist(client, userId, series, notificationDate, token));
    }
    
    public Task<Either<DomainError, Unit>> Upsert(MoviesSubscriptionStorage entity, CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client =>
            TableUtils.TryExecute(() => client.UpsertEntityAsync(entity, cancellationToken: token))
                .MapAsync(_ => unit));
    }

    public Task<Either<DomainError, Unit>> BulkUpdate(ImmutableList<MoviesSubscriptionStorage> entities, CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client =>
            TableUtils.BatchAdd(client,entities,token)
                .MapAsync(_ => unit));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, RowKey series,
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