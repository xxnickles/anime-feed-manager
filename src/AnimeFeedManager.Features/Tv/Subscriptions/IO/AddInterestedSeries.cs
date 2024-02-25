using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IAddInterested
{
    public Task<Either<DomainError, Unit>> Add(UserId userId, RowKey seriesId, NoEmptyString series,
        CancellationToken token);
}

public sealed class AddInterested(ITableClientFactory<InterestedStorage> clientFactory) : IAddInterested
{
    public Task<Either<DomainError, Unit>> Add(UserId userId, RowKey seriesId, NoEmptyString series,
        CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => Persist(client, userId, seriesId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, RowKey seriesId, 
        NoEmptyString series,
        CancellationToken token)
    {
        var storage = new InterestedStorage
        {
            SeriesId = seriesId,
            PartitionKey = userId,
            RowKey = series,
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}