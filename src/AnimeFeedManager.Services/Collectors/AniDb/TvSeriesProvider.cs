using System.Collections.Immutable;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;

namespace AnimeFeedManager.Services.Collectors.AniDb;

public class TvSeriesProvider : ITvSeriesProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    public TvSeriesProvider(
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, TvSeries>> GetLibrary(ImmutableList<string> feedTitles)
    {
        try
        {
            var (series, season) =
                await Scrapper.Scrap("https://anidb.net/anime/season/?type.tvseries=1", _puppeteerOptions);

            await _domainPostman.SendMessage(new SeasonProcessNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                new SeasonInfoDto(season.Season, season.Year),
                SeriesType.Tv,
                $"{series.Count()} series have been scrapped for {season.Season}-{season.Year}"));

            return new TvSeries(series.Select(i => Mappers.Map(i, feedTitles))
                    .ToImmutableList(),
                series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(Mappers.MapImages)
                    .ToImmutableList());
        }
        catch (Exception ex)
        {
            await _domainPostman.SendMessage(
                new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSeasonInfo(),
                    SeriesType.Tv,
                    "AniDb season scrapping failed"));
            return ExceptionError.FromException(ex, "LiveChartLibrary");
        }
    }
}