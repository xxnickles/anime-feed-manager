using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Types;
using AnimeFeedManager.Features.Seasons;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Ovas.Scrapping;

public sealed class OvasLibraryUpdater
{
    private readonly IMediator _mediator;
    private readonly IOvasProvider _ovasProvider;
    private readonly IOvasStorage _ovasStorage;

    public OvasLibraryUpdater(
        IMediator mediator,
        IOvasProvider ovasProvider,
        IOvasStorage ovasStorage)
    {
        _mediator = mediator;
        _ovasProvider = ovasProvider;
        _ovasStorage = ovasStorage;
    }


    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, CancellationToken token = default)
    {
        return _ovasProvider.GetLibrary(season, token)
            .BindAsync(series => Persist(series, season, token));
    }


    private Task<Either<DomainError, Unit>> Persist(OvasCollection series, SeasonSelector seasonSelector,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return _ovasStorage.Add(series.SeriesList, token)
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