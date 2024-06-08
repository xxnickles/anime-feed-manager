using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Features.State.IO;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed;

public sealed class OvaFeedUpdater
{
    private readonly IOvaFeedScrapper _feedScrapper;
    private readonly IStateCreator _stateCreator;
    private readonly IDomainPostman _domainPostman;

    public OvaFeedUpdater(
        IOvaFeedScrapper feedScrapper,
        IStateCreator stateCreator,
        IDomainPostman domainPostman)
    {
        _feedScrapper = feedScrapper;
        _stateCreator = stateCreator;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, int>> TryGetFeed(ImmutableList<OvaStorage> ovas,
        CancellationToken token)
    {
        return _feedScrapper.GetFeed(ovas, token)
            .BindAsync(CreateState)
            .BindAsync(data => SendMessages(data, token));
    }

    private Task<Either<DomainError, ImmutableList<StateWrap<UpdateOvaFeed>>>> CreateState(
        ImmutableList<(OvaStorage Ova, ImmutableList<SeriesFeedLinks> Links)> data)
    {
        return _stateCreator.Create(
            NotificationTarget.Ova,
            data.ConvertAll(info => new UpdateOvaFeed(info.Ova, info.Links)),
            new Box(UpdateOvaFeed.TargetQueue));
    }

    private Task<Either<DomainError, int>> SendMessages(ImmutableList<StateWrap<UpdateOvaFeed>> data,
        CancellationToken token)
    {
        return
            Task.WhenAll(data.Select(info => _domainPostman.SendMessage(info, token)))
                .FlattenResults()
                .MapAsync(r => r.Count);
    }
}