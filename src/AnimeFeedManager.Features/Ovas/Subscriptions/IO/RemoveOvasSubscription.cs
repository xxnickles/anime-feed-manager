using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IRemoveOvasSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, RowKey series, CancellationToken token);
}

public sealed class RemoveOvasSubscription(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
    : IRemoveOvasSubscription
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