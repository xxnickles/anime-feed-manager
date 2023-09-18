using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveTvSubscription
{
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class RemoveTvSubscription : IRemoveTvSubscription
{
    private readonly ITableClientFactory<SubscriptionStorage> _clientFactory;

    public RemoveTvSubscription(ITableClientFactory<SubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public Task<Either<DomainError, Unit>> Unsubscribe(UserId userId, NoEmptyString series, CancellationToken token)
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