using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IGetOvasSubscriptions
{
    Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token);
}

public class GetOvasSubscriptions(ITableClientFactory<OvasSubscriptionStorage> clientFactory) : IGetOvasSubscriptions
{
    public Task<Either<DomainError, ImmutableList<string>>> GetSubscriptions(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(storage => storage.PartitionKey == userId,
                    cancellationToken: token)))
            .MapAsync(subscriptions => subscriptions.ConvertAll(s => s.RowKey ?? string.Empty));
    }
}