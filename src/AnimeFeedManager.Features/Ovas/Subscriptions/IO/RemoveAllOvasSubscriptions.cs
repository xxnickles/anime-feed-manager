using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public interface IRemoveAllOvasSubscriptions
{
    Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token);
}

public class RemoveAllOvasSubscriptions : IRemoveAllOvasSubscriptions
{
    private readonly ITableClientFactory<OvasSubscriptionStorage> _clientFactory;

    public RemoveAllOvasSubscriptions(ITableClientFactory<OvasSubscriptionStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => RemoveAllSubscription(client, userId, token));
    }

    private Task<Either<DomainError, ProcessResult>> RemoveAllSubscription(TableClient client, UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvasSubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token))
            .BindAsync(items => RemoveAll(client, items, token));
    }

    private static Task<Either<DomainError, ProcessResult>> RemoveAll(TableClient client,
        ImmutableList<OvasSubscriptionStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty) return Task.FromResult(Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.OvasSubscriptions)));

        return TableUtils.BatchDelete(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count, ProcessScope.OvasSubscriptions));
    }
}