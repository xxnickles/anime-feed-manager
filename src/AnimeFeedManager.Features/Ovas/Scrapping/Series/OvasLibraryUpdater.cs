using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Series;

public sealed class OvasLibraryUpdater(
    IOvasStatusProvider statusProvider,
    IDomainPostman domainPostman,
    IOvasProvider ovasProvider,
    IOvasStorage ovasStorage)
{
    public Task<Either<DomainError, Unit>> Update(SeasonSelector season, bool keepStatus,
        CancellationToken token = default)
    {
        return ovasProvider.GetLibrary(season, token)
            .BindAsync(series => Persist(series, season, keepStatus, token));
    }


    private Task<Either<DomainError, Unit>> Persist(OvasCollection series, SeasonSelector seasonSelector,
        bool keepStatus,
        CancellationToken token)
    {
        var reference = series.SeriesList.First();
        var result = keepStatus
            ? PersistUsingExistentStatus(series.SeriesList, series.SeasonInformation, token)
            : ovasStorage.Add(series.SeriesList, token);

        return result.BindAsync(_ => CreateImageEvents(series.Images, token))
            .BindAsync(_ => CreateSeasonEvent(reference.Season!, reference.Year, seasonSelector.IsLatest(), token));
    }

    private Task<Either<DomainError, Unit>> PersistUsingExistentStatus(
        ImmutableList<OvaStorage> ovas,
        SeasonInformation seasonInformation,
        CancellationToken token)
    {
        return statusProvider.GetSeasonSeriesStatus(seasonInformation.Season, seasonInformation.Year, token)
            .MapAsync(statuses => ovas.ConvertAll(s => ApplyExistentStatus(s, statuses)))
            .BindAsync(series => ovasStorage.Add(series, token));
    }

    private static OvaStorage ApplyExistentStatus(OvaStorage ova, ImmutableList<OvaFeedStatus> statusList)
    {
        OvaFeedStatus? oldStatus = statusList.FirstOrDefault(sl => sl.Id == ova.RowKey);
        if (oldStatus != null)
        {
            ova.Status = oldStatus?.Status;
        }

        return ova;
    }

    private Task<Either<DomainError, Unit>> CreateImageEvents(ImmutableList<DownloadImageEvent> events,
        CancellationToken token)
    {
        // Publish event to scrap images
        return domainPostman.SendMessage(new ScrapImagesRequest(events), token);
    }

    private Task<Either<DomainError, Unit>> CreateSeasonEvent(string season, int year, bool isLatest,
        CancellationToken token)
    {
        return domainPostman.SendMessage(new AddSeasonNotification(season, year, isLatest), token);
    }
}