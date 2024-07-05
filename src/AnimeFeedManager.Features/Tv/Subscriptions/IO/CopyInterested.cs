using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface ICopyInterested
{
    Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token);
}

public class CopyInterested : ICopyInterested
{
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public CopyInterested(ITableClientFactory<InterestedStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ProcessResult>> CopyAll(UserId source, UserId target, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => CopySubscription(client, source, target, token));
    }


    private static Task<Either<DomainError, ProcessResult>> CopySubscription(TableClient client, UserId source, UserId target,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<InterestedStorage>(s => s.PartitionKey == source, cancellationToken: token))
            .BindAsync(items => StoreAll(client, items.ConvertAll(i => ReMap(i, target)), token));
    }

    private static Task<Either<DomainError, ProcessResult>> StoreAll(TableClient client,
        ImmutableList<InterestedStorage> interested,
        CancellationToken token)
    {
        if (interested.IsEmpty)
            return Task.FromResult(
                Right<DomainError, ProcessResult>(new ProcessResult(0, ProcessScope.TvInterested)));

        return TableUtils.BatchAdd(client, interested, token)
            .MapAsync(_ => new ProcessResult((ushort) interested.Count, ProcessScope.TvInterested));
    }

    private static InterestedStorage ReMap(InterestedStorage interestedStorage, UserId target)
    {
        interestedStorage.PartitionKey = target;
        return interestedStorage;
    }
}