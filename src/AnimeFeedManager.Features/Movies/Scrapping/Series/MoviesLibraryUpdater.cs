using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Series.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Movies.Scrapping.Series;

public sealed class MoviesLibraryUpdater(
    IMoviesStatusProvider statusProvider,
    IDomainPostman domainPostman,
    IMoviesProvider moviesProvider,
    IMoviesStorage moviesStorage)
{
    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, bool keepStatus,
        CancellationToken token = default)
    {
        return moviesProvider.GetLibrary(season, token)
            .BindAsync(series => Persist(series, season, keepStatus, token));
    }

    private Task<Either<DomainError, Unit>> Persist(MoviesCollection series, SeasonSelector seasonSelector,
        bool keepStatus,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        var result = keepStatus
            ? PersistUsingExistentStatus(series.SeriesList, series.SeasonInformation, token)
            : moviesStorage.Add(series.SeriesList, token);

        return result.BindAsync(_ => CreateImageEvents(series.Images, token))
            .BindAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year, seasonSelector.IsLatest(), token));
    }

    private Task<Either<DomainError, Unit>> PersistUsingExistentStatus(
        ImmutableList<MovieStorage> movies,
        SeasonInformation seasonInformation,
        CancellationToken token)
    {
        return statusProvider.GetSeasonSeriesStatus(seasonInformation.Season, seasonInformation.Year, token)
            .MapAsync(statuses => movies.ConvertAll(s => ApplyExistentStatus(s, statuses)))
            .BindAsync(series => moviesStorage.Add(series, token));
    }


    private static MovieStorage ApplyExistentStatus(MovieStorage movie, ImmutableList<MovieFeedStatus> statusList)
    {
        MovieFeedStatus? oldStatus = statusList.FirstOrDefault(sl => sl.Id == movie.RowKey);
        if (oldStatus != null)
        {
            movie.Status = oldStatus?.Status;
        }

        return movie;
    }

    private Task<Either<DomainError, Unit>> CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        return domainPostman.SendMessage(new ScrapImagesRequest(events), token);
    }

    private Task<Either<DomainError, Unit>> CreateSeasonEvent(string season, int year, bool isLatest,
        CancellationToken token)
    {
        return domainPostman.SendMessage(new AddSeasonNotification(season, year, isLatest), token);
    }
}