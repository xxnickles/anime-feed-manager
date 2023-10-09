using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO
{
    public interface IGetMovieSubscriptions
    {
        Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token);
    }

    public class GetMovieSubscriptions : IGetMovieSubscriptions
    {
        private readonly ITableClientFactory<MoviesSubscriptionStorage> _clientFactory;

        public GetMovieSubscriptions(ITableClientFactory<MoviesSubscriptionStorage> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token)
        {
            return _clientFactory.GetClient()
                .BindAsync(client => TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<MoviesSubscriptionStorage>(storage => storage.PartitionKey == userId,
                        cancellationToken: token)))
                .MapAsync(subscriptions => subscriptions.ConvertAll(s => s.RowKey ?? string.Empty));
        }
    }
}