using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.State.IO;
using AnimeFeedManager.Old.Features.State.Types;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed;

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
        return _feedScrapper.GetFeed(ovas)
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