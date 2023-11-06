using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveTvSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class RemoveTvSubscription(ITableClientFactory<SubscriptionStorage> clientFactory) : IRemoveTvSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => Delete(client, userId, series, token));
    }
    
    private static Task<Either<DomainError, Unit>> Delete(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        return TableUtils.TryExecute(() => client.DeleteEntityAsync(userId, series, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}