using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Feed;

public sealed class MovieFeedUpdater
{
    private readonly IMovieFeedScrapper _feedScrapper;
    private readonly IDomainPostman _domainPostman;

    public MovieFeedUpdater(
        IMovieFeedScrapper feedScrapper,
        IDomainPostman domainPostman)
    {
        _feedScrapper = feedScrapper;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, int>> TryGetFeed(ImmutableList<MovieStorage> movies,
        CancellationToken token)
    {
        return _feedScrapper.GetFeed(movies, token)
            .BindAsync(data => SendMessages(data, token));
    }

    private Task<Either<DomainError, int>> SendMessages(ImmutableList<(MovieStorage Movie, ImmutableList<SeriesFeedLinks> Links)> data, CancellationToken token)
    {
        return Task.WhenAll(
                data.Select(info => _domainPostman.SendMessage(new UpdateMovieFeed(info.Movie, info.Links), token)))
            .FlattenResults()
            .MapAsync(r => r.Count);
    }
}