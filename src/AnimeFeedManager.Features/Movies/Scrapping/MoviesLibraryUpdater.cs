using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Movies.Scrapping.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Types;
using AnimeFeedManager.Features.Seasons;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Movies.Scrapping;

public sealed class MoviesLibraryUpdater
{
    private readonly IMediator _mediator;
    private readonly IMoviesProvider _moviesProvider;
    private readonly IMoviesStorage _moviesStorage;

    public MoviesLibraryUpdater(
        IMediator mediator,
        IMoviesProvider moviesProvider,
        IMoviesStorage moviesStorage)
    {
        _mediator = mediator;
        _moviesProvider = moviesProvider;
        _moviesStorage = moviesStorage;
    }


    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, CancellationToken token = default)
    {
        return SeasonValidators.Validate(season)
            .BindAsync(s => _moviesProvider.GetLibrary(s, token))
            .BindAsync(series => Persist(series, season, token));
    }


    private Task<Either<DomainError, Unit>> Persist(MoviesCollection series, SeasonSelector seasonSelector, CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return _moviesStorage.Add(series.SeriesList, token)
            .MapAsync(_ => CreateImageEvents(series.Images, token))
            .MapAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year, seasonSelector.IsLatest()));
    }

    private Unit CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        _mediator.Publish(new ScrapNotificationImages(events), token);
        return unit;
    }
    
    private Unit CreateSeasonEvent(string season, int year, bool isLatest )
    {
        _mediator.Publish(new AddSeasonNotification(season, year, isLatest));
        return unit;
    }
}