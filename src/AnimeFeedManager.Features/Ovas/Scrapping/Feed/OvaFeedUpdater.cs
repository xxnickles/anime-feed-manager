using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed;

public sealed class OvaFeedUpdater
{
    private readonly IOvaFeedScrapper _feedScrapper;
    private readonly IDomainPostman _domainPostman;

    public OvaFeedUpdater(
        IOvaFeedScrapper feedScrapper,
        IDomainPostman domainPostman)
    {
        _feedScrapper = feedScrapper;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, int>> TryGetFeed(ImmutableList<OvaStorage> ovas,
        CancellationToken token)
    {
        return _feedScrapper.GetFeed(ovas, token)
            .BindAsync(data => SendMessages(data, token));
    }

    private Task<Either<DomainError, int>> SendMessages(ImmutableList<(OvaStorage Ova,ImmutableList<OvaFeedLinks> Links)> data, CancellationToken token)
    {
        return Task.WhenAll(
                data.Select(info => _domainPostman.SendMessage(new UpdateOvaFeed(info.Ova, info.Links), token)))
            .FlattenResults()
            .MapAsync(r => r.Count);
    }
}