using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface ICopyOvasSubscriptions
{
    Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token);
}

public class CopyOvasSubscriptions : ICopyOvasSubscriptions
{
    private readonly ITableClientFactory<OvasSubscriptionStorage> _clientFactory;

    public CopyOvasSubscriptions(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => CopySubscription(client, source, target, token));
    }


    private Task<Either<DomainError, ProcessResult>> CopySubscription(TableClient client, UserId source, UserId target,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(s => s.PartitionKey == source, cancellationToken: token))
            .BindAsync(items => StoreAll(client, items.ConvertAll(i => ReMap(i, target)), token));
    }

    private static Task<Either<DomainError, ProcessResult>> StoreAll(TableClient client,
        ImmutableList<OvasSubscriptionStorage> ovasSubscriptions,
        CancellationToken token)
    {
        if (ovasSubscriptions.IsEmpty)
            return Task.FromResult(
                Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.OvasSubscriptions)));

        return TableUtils.BatchAdd(client, ovasSubscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) ovasSubscriptions.Count, ProcessScope.OvasSubscriptions));
    }

    private static OvasSubscriptionStorage ReMap(OvasSubscriptionStorage ovasSubscriptionsStorage, UserId target)
    {
        ovasSubscriptionsStorage.PartitionKey = target;
        return ovasSubscriptionsStorage;
    }
}