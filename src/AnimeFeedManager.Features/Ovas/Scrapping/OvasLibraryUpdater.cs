using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Types;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Ovas.Scrapping;

public sealed class OvasLibraryUpdater(
    IDomainPostman domainPostman,
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
            .BindAsync(_ => CreateImageEvents(series.Images, token))
            .BindAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year, seasonSelector.IsLatest(), token));
    }

    private Task<Either<DomainError, Unit>> CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        return domainPostman.SendMessage(new ScrapImagesRequest(events), Box.ImageToScrap, token);
    }

    private Task<Either<DomainError, Unit>> CreateSeasonEvent(string season, int year, bool isLatest,
        CancellationToken token)
    {
        return domainPostman.SendMessage(new AddSeasonNotification(season, year, isLatest), Box.AddSeason, token);
    }
}