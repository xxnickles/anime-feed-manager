using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Titles;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping;

public sealed class UpdateLatestLibrary
{
    private readonly IMediator _mediator;
    private readonly ISeriesProvider _seriesProvider;
    private readonly ITitlesProvider _titlesProvider;

    public UpdateLatestLibrary(
        IMediator mediator,
        ISeriesProvider seriesProvider,
        ITitlesProvider titlesProvider)
    {
        _mediator = mediator;
        _seriesProvider = seriesProvider;
        _titlesProvider = titlesProvider;
    }

    public Task<Either<DomainError, Unit>> Update()
    {
        return _seriesProvider.GetLibrary()
            .BindAsync(TryAddFeedTitles)
            .BindAsync(Persist);
    }

    private Task<Either<DomainError, TvSeries>> TryAddFeedTitles(TvSeries series)
    {
        return _titlesProvider.GetTitles()
            .MapAsync(UpdateTitles)
            .MapAsync(titles =>
            {
                var updatedSeries =
                    series.SeriesList.ConvertAll(s => s with { FeedTitle = Utils.TryGetFeedTitle(titles, s.Title) });

                return series with { SeriesList = updatedSeries };
            });
    }

    private ImmutableList<string> UpdateTitles(ImmutableList<string> titles)
    {
        // Publish event to update titles
        _mediator.Publish(new UpdateSeasonTitles(titles));
        return titles;
    }

    private Task<Either<DomainError, Unit>> Persist(TvSeries series)
    {
        throw new NotImplementedException();
    }
}