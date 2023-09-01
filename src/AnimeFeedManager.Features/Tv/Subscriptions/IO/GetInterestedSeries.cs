using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IGetInterestedSeries
{
    Task<Either<DomainError, ImmutableList<InterestedStorage>>> Get(UserId userId, CancellationToken token);
}

public sealed class GetInterestedSeries : IGetInterestedSeries
{
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public GetInterestedSeries(ITableClientFactory<InterestedStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, ImmutableList<InterestedStorage>>> Get(UserId userId, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<InterestedStorage>(i => i.PartitionKey == userId, cancellationToken: token)));
    }
}