using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Dto;
using AnimeFeedManager.Features.Common.Types;
using AnimeFeedManager.Features.Common.Utils;
using AnimeFeedManager.Features.Domain;
using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Infrastructure.Notifications;
using AnimeFeedManager.Features.Tv.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public class SeriesProvider : ISeriesProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    public SeriesProvider(
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, TvSeries>> GetLibrary()
    {
        try
        {
            var (series, season) =
                await AniDbScrapper.Scrap("https://anidb.net/anime/season/?type.tvseries=1", _puppeteerOptions);

            await _domainPostman.SendMessage(new SeasonProcessNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                new SeasonInfoDto(season.Season, season.Year),
                SeriesType.Tv,
                $"{series.Count()} series have been scrapped for {season.Season}-{season.Year}"));

            return new TvSeries(series.Select(MapInfo)
                    .ToImmutableList(),
                series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(AniDbMappers.MapImages)
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
            return ExceptionError.FromException(ex, "AniDbLibrary");
        }
    }

    private static AnimeInfo MapInfo(SeriesContainer container)
    {
        return new AnimeInfo(
            container.Id,
            container.Title,
            container.Synopsys,
            string.Empty,
            MapSeasonInfo(container.SeasonInfo),
            MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year),
            false
        );
    }

    private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}