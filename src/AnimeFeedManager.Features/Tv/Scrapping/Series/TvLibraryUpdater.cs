using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Seasons;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public sealed class TvLibraryUpdater
{
    private readonly IMediator _mediator;
    private readonly ISeriesProvider _seriesProvider;
    private readonly ITitlesProvider _titlesProvider;
    private readonly ITvSeriesStore _seriesStore;

    public TvLibraryUpdater(
        IMediator mediator,
        ISeriesProvider seriesProvider,
        ITitlesProvider titlesProvider,
        ITvSeriesStore seriesStore)
    {
        _mediator = mediator;
        _seriesProvider = seriesProvider;
        _titlesProvider = titlesProvider;
        _seriesStore = seriesStore;
    }

    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, CancellationToken token = default)
    {
        return SeasonValidators.Validate(season)
            .BindAsync(s => _seriesProvider.GetLibrary(s, token))
            .BindAsync(series => TryAddFeedTitles(series, token))
            .BindAsync(series => Persist(series, token));
    }

    private Task<Either<DomainError, TvSeries>> TryAddFeedTitles(TvSeries series, CancellationToken token)
    {
        return _titlesProvider.GetTitles()
            .MapAsync(titles => UpdateTitles(titles, token))
            .MapAsync(titles =>
            {
                var updatedSeries =
                    series.SeriesList.ConvertAll(s =>
                    {
                        s.FeedTitle = Utils.TryGetFeedTitle(titles, s.Title ?? string.Empty);
                        return s;
                    });

                return series with { SeriesList = updatedSeries };
            });
    }

    private ImmutableList<string> UpdateTitles(ImmutableList<string> titles, CancellationToken token)
    {
        // Publish event to update titles
        _mediator.Publish(new UpdateSeasonTitles(titles), token);
        return titles;
    }

    private Task<Either<DomainError, Unit>> Persist(TvSeries series, CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return _seriesStore.Add(series.SeriesList, token)
            .MapAsync(_ => CreateImageEvents(series.Images, token))
            .MapAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year));
    }

    private Unit CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        _mediator.Publish(new ScrapNotificationImages(events), token);
        return unit;
    }

    private Unit CreateSeasonEvent(string season, int year)
    {
        _mediator.Publish(new AddSeasonNotification(season, year));
        return unit;
    }
}