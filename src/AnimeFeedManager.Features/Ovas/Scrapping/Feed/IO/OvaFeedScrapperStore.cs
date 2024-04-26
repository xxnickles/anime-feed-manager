using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;

public interface IOvaFeedStore
{
    Task<Either<DomainError, Unit>> Add(OvaFeedStorage feed, CancellationToken token);
}

public sealed class OvaFeedStore : IOvaFeedStore
{
    private readonly ITableClientFactory<OvaFeedStorage> _tableClientFactory;

    public OvaFeedStore(ITableClientFactory<OvaFeedStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> Add(OvaFeedStorage feed, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(feed, TableUpdateMode.Merge, token)))
            .MapAsync(_ => unit);
    }
}