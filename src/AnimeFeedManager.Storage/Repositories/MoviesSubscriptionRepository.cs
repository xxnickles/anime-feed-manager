using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class MoviesSubscriptionRepository : IMoviesSubscriptionRepository
{
    private readonly TableClient _tableClient;

    public MoviesSubscriptionRepository(ITableClientFactory<MoviesSubscriptionStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> Get(Email userEmail)
    {
        var user = userEmail.Value.UnpackOption(string.Empty);
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<MoviesSubscriptionStorage>(s => s.PartitionKey == user), nameof(MoviesSubscriptionStorage));
    }

    public Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetTodaySubscriptions()
    {
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<MoviesSubscriptionStorage>(
                s => s.DateToNotify >= DateTime.Today), nameof(MoviesSubscriptionStorage));
    }

    public Task<Either<DomainError, Unit>> Complete(string subscriber, string title)
    {
        MoviesSubscriptionStorage MarkCompleted(MoviesSubscriptionStorage storage)
        {
            storage.Processed = true;
            return storage;
        }
        
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<MoviesSubscriptionStorage>(s =>
                    s.PartitionKey == subscriber && s.RowKey == title), nameof(MoviesSubscriptionStorage))
            .MapAsync(x => MarkCompleted(x.First()))
            
            .BindAsync(Merge);
    }

    public Task<Either<DomainError, Unit>> Merge(MoviesSubscriptionStorage subscription)
    {
        return TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(subscription),
            nameof(MovieStorage)).MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> Delete(string subscriber, string title)
    {
        return TableUtils.TryExecute(() =>
                    _tableClient.DeleteEntityAsync(subscriber, title),
                nameof(SubscriptionStorage))
            .MapAsync(_ => unit);
    }
}