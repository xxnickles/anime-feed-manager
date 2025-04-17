using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Movies.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Movies.Subscriptions;

public sealed class MoviesSubscriptionStatusResetter
{
    private readonly IGetMovieSubscriptions _movieSubscriptions;
    private readonly IMovieSubscriptionStore _subscriptionStore;

    public MoviesSubscriptionStatusResetter(
        IGetMovieSubscriptions  movieSubscriptions,
        IMovieSubscriptionStore subscriptionStore)
    {
        _movieSubscriptions = movieSubscriptions;
        _subscriptionStore = subscriptionStore;
    }

    public Task<Either<DomainError, Unit>> ResetStatus(RowKey series, CancellationToken token)
    {
        return _movieSubscriptions.GetSubscriptionForMovie(series,token)
            .MapAsync(movieSubscriptions => movieSubscriptions.ConvertAll(ResetSeriesStatus))
            .BindAsync(processedMovies => _subscriptionStore.BulkUpdate(processedMovies, token));
    }


    private MoviesSubscriptionStorage ResetSeriesStatus(MoviesSubscriptionStorage moviesSubscriptionStorage)
    {
        moviesSubscriptionStorage.Processed = false;
        return moviesSubscriptionStorage;
    }
}