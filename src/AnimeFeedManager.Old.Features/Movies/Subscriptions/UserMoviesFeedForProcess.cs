using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Movies.Subscriptions;

public readonly record struct MoviesUserFeed(
    ImmutableList<FeedProcessedMovie> Feed,
    ImmutableList<MoviesSubscriptionStorage> Subscriptions);

public sealed class UserMoviesFeedForProcess
{
    private readonly IGetProcessedMovies _getProcessedMovies;
    private readonly IGetMovieSubscriptions _getMoviesSubscriptions;

    public UserMoviesFeedForProcess(IGetProcessedMovies getProcessedMovies, IGetMovieSubscriptions getMoviesSubscriptions)
    {
        _getProcessedMovies = getProcessedMovies;
        _getMoviesSubscriptions = getMoviesSubscriptions;
    }

    public Task<Either<DomainError, MoviesUserFeed>> GetFeedForProcess(UserId userId, PartitionKey partitionKey,
        CancellationToken token)
    {
        return _getProcessedMovies.GetForSeason(partitionKey, token)
            .BindAsync(feeds => _getMoviesSubscriptions.GetCompleteSubscriptions(userId, token)
                .MapAsync(subscriptions => new MoviesUserFeed(feeds, subscriptions)))
            .MapAsync(Filter);
    }

    private static MoviesUserFeed Filter(MoviesUserFeed userFeed)
    {
        var feedTitles = userFeed.Feed.Select(f => f.SeriesTitle);
        var subscriptionsTitles = userFeed.Subscriptions.Select(s => s.RowKey);
        var matches = feedTitles.Intersect(subscriptionsTitles);

        if (!matches.Any())
            return new MoviesUserFeed(ImmutableList<FeedProcessedMovie>.Empty,
                ImmutableList<MoviesSubscriptionStorage>.Empty);

        return new MoviesUserFeed(userFeed.Feed.Where(f => matches.Contains(f.SeriesTitle)).ToImmutableList(),
            userFeed.Subscriptions.Where(s => matches.Contains(s.RowKey)).ToImmutableList());
    }
}