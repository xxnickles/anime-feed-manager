using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO
{
    public interface IGetOvasSubscriptions
    {
        Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token);
    }

    public class GetOvasSubscriptions : IGetOvasSubscriptions
    {
        private readonly ITableClientFactory<OvasSubscriptionStorage> _clientFactory;

        public GetOvasSubscriptions(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token)
        {
            return _clientFactory.GetClient()
                .BindAsync(client => TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<OvasSubscriptionStorage>(storage => storage.PartitionKey == userId,
                        cancellationToken: token)))
                .MapAsync(subscriptions => subscriptions.ConvertAll(s => s.RowKey ?? string.Empty));
        }
    }
}