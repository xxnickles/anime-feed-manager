using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Features.State.IO;

namespace AnimeFeedManager.Features.Movies.Scrapping.Feed;

public sealed class MovieFeedUpdater
{
    private readonly IMovieFeedScrapper _feedScrapper;
    private readonly IStateCreator _stateCreator;
    private readonly IDomainPostman _domainPostman;

    public MovieFeedUpdater(
        IMovieFeedScrapper feedScrapper,
        IStateCreator stateCreator,
        IDomainPostman domainPostman)
    {
        _feedScrapper = feedScrapper;
        _stateCreator = stateCreator;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, int>> TryGetFeed(ImmutableList<MovieStorage> movies,
        CancellationToken token)
    {
        return _feedScrapper.GetFeed(movies)
            .BindAsync(CreateState)
            .BindAsync(data => SendMessages(data, token));
    }
    
    private Task<Either<DomainError, ImmutableList<StateWrap<UpdateMovieFeed>>>> CreateState(
        ImmutableList<(MovieStorage Movie, ImmutableList<SeriesFeedLinks> Links)> data)
    {
        return _stateCreator.Create(
            NotificationTarget.Movie,
            data.ConvertAll(info => new UpdateMovieFeed(info.Movie, info.Links)),
            new Box(UpdateMovieFeed.TargetQueue));
    }

    private Task<Either<DomainError, int>> SendMessages(ImmutableList<StateWrap<UpdateMovieFeed>> data, CancellationToken token)
    {
        return Task.WhenAll(
                data.Select(info => _domainPostman.SendMessage(info, token)))
            .FlattenResults()
            .MapAsync(r => r.Count);
    }
}