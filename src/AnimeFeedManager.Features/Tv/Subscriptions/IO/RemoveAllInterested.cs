using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveAllInterested
{
    Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token);
}

public class RemoveAllInterested : IRemoveAllInterested
{
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public RemoveAllInterested(ITableClientFactory<InterestedStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> UnsubscribeAll(UserId userId, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => RemoveAllSubscription(client, userId, token));
    }

    private static Task<Either<DomainError, ProcessResult>> RemoveAllSubscription(TableClient client, UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<InterestedStorage>(s => s.PartitionKey == userId, cancellationToken: token))
            .BindAsync(items => RemoveAll(client, items, token));
    }

    private static Task<Either<DomainError, ProcessResult>> RemoveAll(TableClient client,
        ImmutableList<InterestedStorage> subscriptions,
        CancellationToken token)
    {
        if (subscriptions.IsEmpty) return Task.FromResult(Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.TvInterested)));

        return TableUtils.BatchDelete(client, subscriptions, token)
            .MapAsync(_ => new ProcessResult((ushort) subscriptions.Count, ProcessScope.TvInterested));
    }
}