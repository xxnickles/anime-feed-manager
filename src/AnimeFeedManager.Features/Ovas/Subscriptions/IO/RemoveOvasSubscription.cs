using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IRemoveOvasSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class RemoveOvasSubscription : IRemoveOvasSubscription
{
    private readonly ITableClientFactory<OvasSubscriptionStorage> _clientFactory;

    public RemoveOvasSubscription(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
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