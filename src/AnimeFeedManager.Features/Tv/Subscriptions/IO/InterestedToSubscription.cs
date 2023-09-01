using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IInterestedToSubscription
{
    public Task<Either<DomainError, Unit>> ProcessInterested(UserId userId, CancellationToken token);
}

public sealed class InterestedToSubscription : IInterestedToSubscription
{
    private readonly ITittlesGetter _tittlesGetter;
    private readonly ITableClientFactory<InterestedStorage> _clientFactory;

    public InterestedToSubscription(ITittlesGetter tittlesGetter,
        ITableClientFactory<InterestedStorage> clientFactory)
    {
        _tittlesGetter = tittlesGetter;
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> ProcessInterested(UserId userId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private Task<Either<DomainError, ImmutableList<InterestedStorage>>> GetInterestedSeries(TableClient client,
        UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteQuery(() => client.QueryAsync<InterestedStorage>(i => i.PartitionKey == userId));
    }

    private Task<Either<DomainError, ImmutableList<Types.InterestedToSubscription>>> GetSeriesToSubscribe(
        ImmutableList<InterestedStorage> interestedSeries,
        UserId userId,
        CancellationToken token)
    {
      return _tittlesGetter.GetTitles(token)
            .MapAsync(titles => interestedSeries.ConvertAll(interested => (
                    new
                    {
                        FeedTitle = Utils.TryGetFeedTitle(titles, interested.RowKey ?? string.Empty),
                        InterestedTitle = interested.RowKey ?? string.Empty
                    }))
                .Where(x => !string.IsNullOrEmpty(x.FeedTitle))
                .ToImmutableList()
            )
            .MapAsync(data => data.ConvertAll(item =>
                new Types.InterestedToSubscription(userId, item.FeedTitle, item.InterestedTitle)));
    }
}