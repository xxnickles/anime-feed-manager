using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed;

public sealed class OvaFeedUpdater
{
    private readonly IOvaFeedScrapper _feedScrapper;
    private readonly IOvaFeedStore _ovaFeedStore;
    private readonly IOvasStorage _ovasStorage;

    public OvaFeedUpdater(IOvaFeedScrapper feedScrapper,
        IOvaFeedStore ovaFeedStore,
        IOvasStorage ovasStorage)
    {
        _feedScrapper = feedScrapper;
        _ovaFeedStore = ovaFeedStore;
        _ovasStorage = ovasStorage;
    }

    public Task<Either<DomainError, OvaFeedScrapResult>> TryGetFeed(OvaStorage ovaStorage, CancellationToken token)
    {
        return _feedScrapper.GetFeed(ovaStorage.Title ?? "nope", token)
            .BindAsync(links => Evaluator(ovaStorage, links, token));
    }

    private Task<Either<DomainError, OvaFeedScrapResult>> Evaluator(OvaStorage storage,
        ImmutableList<OvaFeedLinks> links, CancellationToken token)
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
        ImmutableList<OvaFeedLinks> links, CancellationToken token)
    {
        storage.Status = ShortSeriesStatus.Processed;
        return _ovaFeedStore.Add(new OvaFeedStorage
            {
                PartitionKey = storage.PartitionKey,
                RowKey = storage.RowKey,
                Payload = JsonSerializer.Serialize(links.ToArray(), OvasFeedLinksContext.Default.OvasLinkArray)
            }, token)
            .BindAsync(_ => _ovasStorage.Update(storage, token))
            .MapAsync(_ => OvaFeedScrapResult.FoundAndUpdated);
    }
}