using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Common.Domain.Validators;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Seasons;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Types;
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
            .BindAsync(series => Persist(series, season, token));
    }

    private Task<Either<DomainError, TvSeries>> TryAddFeedTitles(TvSeries series, CancellationToken token)
    {
        return _titlesProvider.GetTitles()
            .MapAsync(titles =>
            {
                var updatedSeries =
                    series.SeriesList.ConvertAll(s =>
                    {
                        var feedTitle = Utils.TryGetFeedTitle(titles, s.Title ?? string.Empty);
                        s.FeedTitle = feedTitle;
                        // If there is an available feed, it is an ongoing series
                        if (!string.IsNullOrEmpty(feedTitle))
                            s.Status = SeriesStatus.Ongoing;
                        return s;
                    });

                return (Series: series with { SeriesList = updatedSeries }, Titles: titles);
            }).MapAsync(param => UpdateTitles(param.Titles, param.Series, token));
    }

    private TvSeries UpdateTitles(ImmutableList<string> titles, TvSeries series, CancellationToken token)
    {
        // Publish event to update titles
        _mediator.Publish(new UpdateSeasonTitles(titles), token);
        return series;
    }

    private Task<Either<DomainError, Unit>> Persist(TvSeries series, SeasonSelector seasonSelector,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return _seriesStore.Add(series.SeriesList, token)
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

    private Unit CreateSeasonEvent(string season, int year, bool isLatest)
    {
        _mediator.Publish(new AddSeasonNotification(season, year, isLatest));
        return unit;
    }
}