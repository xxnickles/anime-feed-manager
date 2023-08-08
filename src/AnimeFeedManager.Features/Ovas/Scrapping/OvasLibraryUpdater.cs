using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Types;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Ovas.Scrapping;

public class OvasLibraryUpdater
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
        return SeasonValidators.Validate(season)
            .BindAsync(s => _ovasProvider.GetLibrary(s, token))
            .BindAsync(series => Persist(series, token));
    }


    private Task<Either<DomainError, Unit>> Persist(OvasCollection series, CancellationToken token)
    {
        return _ovasStorage.Add(series.SeriesList, token)
            .MapAsync(_ => CreateImageEvents(series.Images, token));
    }

    private Unit CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        _mediator.Publish(new ScrapNotificationImages(events), token);
        return unit;
    }
}