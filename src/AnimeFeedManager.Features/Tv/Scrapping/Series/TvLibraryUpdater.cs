using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series;

public sealed class TvLibraryUpdater(
    IMediator mediator,
    IDomainPostman domainPostman,
    ISeriesProvider seriesProvider,
    ITitlesProvider titlesProvider,
    ITvSeriesStore seriesStore)
{
    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, CancellationToken token = default)
    {
        return seriesProvider.GetLibrary(season, token)
            .BindAsync(series => TryAddFeedTitles(series, token))
            .BindAsync(series => Persist(series, season, token));
    }

    private Task<Either<DomainError, TvSeries>> TryAddFeedTitles(TvSeries series, CancellationToken token)
    {
        return titlesProvider.GetTitles()
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

                return (Series: series with {SeriesList = updatedSeries}, Titles: titles);
            }).MapAsync(param => UpdateTitles(param.Titles, param.Series, token));
    }

    private TvSeries UpdateTitles(ImmutableList<string> titles, TvSeries series, CancellationToken token)
    {
        // Publish event to update titles
        domainPostman.SendMessage(new UpdateSeasonTitlesRequest(titles), Box.SeasonTitlesProcess, token);
        return series;
    }

    private Task<Either<DomainError, Unit>> Persist(TvSeries series, SeasonSelector seasonSelector,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return seriesStore.Add(series.SeriesList, token)
            .MapAsync(_ => CreateImageEvents(series.Images, token))
            .MapAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year, seasonSelector.IsLatest()));
    }

    private Unit CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        mediator.Publish(new ScrapNotificationImages(events), token);
        return unit;
    }

    private Unit CreateSeasonEvent(string season, int year, bool isLatest)
    {
        mediator.Publish(new AddSeasonNotification(season, year, isLatest));
        return unit;
    }
}