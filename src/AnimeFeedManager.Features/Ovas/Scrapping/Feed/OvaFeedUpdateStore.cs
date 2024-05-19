using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed;

public class OvaFeedUpdateStore
{
    private readonly IOvasStorage _ovasStorage;

    public OvaFeedUpdateStore(IOvasStorage ovasStorage)
    {
        _ovasStorage = ovasStorage;
    }

    public Task<Either<DomainError, OvaFeedScrapResult>> StoreFeedUpdates(OvaStorage storage,
        ImmutableList<SeriesFeedLinks> links, CancellationToken token)
    {
        return links.IsEmpty
            ? HandleEmptyLinks(storage, token)
            : ProcessLinks(storage, links, token);
    }

    private Task<Either<DomainError, OvaFeedScrapResult>> HandleEmptyLinks(OvaStorage storage, CancellationToken token)
    {
        storage.Status = ShortSeriesStatus.NotFeedFound;
        return _ovasStorage.Update(storage, token).MapAsync(_ => OvaFeedScrapResult.NotFound);
    }

    private Task<Either<DomainError, OvaFeedScrapResult>> ProcessLinks(OvaStorage storage,
        ImmutableList<SeriesFeedLinks> links, CancellationToken token)
    {
        storage.Status = ShortSeriesStatus.Processed;
        storage.FeedInfo = JsonSerializer.Serialize(links.ToArray(), SeriesFeedLinksContext.Default.SeriesFeedLinksArray);
        return _ovasStorage.Update(storage, token)
            .MapAsync(_ => OvaFeedScrapResult.FoundAndUpdated);
    }
}