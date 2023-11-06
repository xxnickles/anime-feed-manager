using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Types;
using AnimeFeedManager.Features.Seasons;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Ovas.Scrapping;

public sealed class OvasLibraryUpdater(
    IMediator mediator,
    IOvasProvider ovasProvider,
    IOvasStorage ovasStorage)
{
    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, CancellationToken token = default)
    {
        return ovasProvider.GetLibrary(season, token)
            .BindAsync(series => Persist(series, season, token));
    }


    private Task<Either<DomainError, Unit>> Persist(OvasCollection series, SeasonSelector seasonSelector,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        return ovasStorage.Add(series.SeriesList, token)
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