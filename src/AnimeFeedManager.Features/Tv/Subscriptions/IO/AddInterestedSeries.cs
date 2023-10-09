using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IAddInterested
{
    public Task<Either<DomainError, Unit>> Add(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class AddInterested : IAddInterested
{
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public AddInterested(ITableClientFactory<InterestedStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Add(UserId userId, NoEmptyString series, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => Persist(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        var storage = new InterestedStorage
        {
            PartitionKey = userId,
            RowKey = series,
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}