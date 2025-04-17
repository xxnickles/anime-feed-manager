using System.Text.Json;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using SeriesFeedLinksContext = AnimeFeedManager.Old.Common.Domain.Types.SeriesFeedLinksContext;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed;

public sealed class OvaFeedUpdateStore
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