using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IGetInterestedSeries
{
    Task<Either<DomainError, ImmutableList<InterestedStorage>>> Get(UserId userId, CancellationToken token);
}

public sealed class GetInterestedSeries(ITableClientFactory<InterestedStorage> clientFactory) : IGetInterestedSeries
{
    public Task<Either<DomainError, ImmutableList<InterestedStorage>>> Get(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQueryWithEmptyResult(() =>
                    client.QueryAsync<InterestedStorage>(i => i.PartitionKey == userId, cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(AddReplacedCharacters));
    }

    private static InterestedStorage AddReplacedCharacters(InterestedStorage storage)
    {
        storage.RowKey = storage.RowKey?.RestoreForbiddenRowKeyParameters() ?? string.Empty;
        return storage;
    }
}