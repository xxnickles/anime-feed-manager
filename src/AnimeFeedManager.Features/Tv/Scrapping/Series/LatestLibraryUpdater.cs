using AnimeFeedManager.Features.Tv.Scrapping.Images;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series
{
    public sealed class LatestLibraryUpdater
    {
        private readonly IMediator _mediator;
        private readonly ILatestSeriesProvider _latestSeriesProvider;
        private readonly ITitlesProvider _titlesProvider;
        private readonly ITvSeriesStore _seriesStore;

        public LatestLibraryUpdater(
            IMediator mediator,
            ILatestSeriesProvider latestSeriesProvider,
            ITitlesProvider titlesProvider,
            ITvSeriesStore seriesStore)
        {
            _mediator = mediator;
            _latestSeriesProvider = latestSeriesProvider;
            _titlesProvider = titlesProvider;
            _seriesStore = seriesStore;
        }

        public Task<Either<DomainError, Unit>> Update(CancellationToken token = default)
        {
            return _latestSeriesProvider.GetLibrary(token)
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
                            s.FeedTitle = Utils.TryGetFeedTitle(titles, s.Title);
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
            return _seriesStore.Add(series.SeriesList, token)
                .MapAsync(_ => CreateImageEvents(series.Images, token));
        }

        private async Task<Unit> CreateImageEvents(ImmutableList<DownloadImageEvent> events,
            CancellationToken token)
        {
            // Publish event to scrap images
            _mediator.Publish(new ScrapNotificationImages(events), token);
            return unit;
        }
    }
}