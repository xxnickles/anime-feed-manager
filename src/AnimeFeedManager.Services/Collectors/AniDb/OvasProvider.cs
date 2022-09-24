using System.Collections.Immutable;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Services.Collectors.AniDb;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;
using Mappers = AnimeFeedManager.Services.Collectors.AniDb.Mappers;

namespace AnimeFeedManager.Services.Collectors.AniChart;

public class OvasProvider : IOvasProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    public OvasProvider(
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public Task<Either<DomainError, Ovas>> GetLibrary()
    {
        return Process("https://anidb.net/anime/season/?type.ova=1&type.tvspecial=1&type.web=1");
    }

    public Task<Either<DomainError, Ovas>> GetLibrary(SeasonInformation seasonInformation)
    {
        var (season,year) = seasonInformation.UnwrapForAniDb();
        return Process($"https://anidb.net/anime/season/{season}/{year}?type.ova=1&type.tvspecial=1&type.web=1");
    }

    private async Task<Either<DomainError, Ovas>> Process(string url)
    {
        try
        {
            var (series, season) =
                await Scrapper.Scrap(url, _puppeteerOptions);

            await _domainPostman.SendMessage(new SeasonProcessNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                new SeasonInfoDto(season.Season, season.Year),
                SeriesType.Ova,
                $"{series.Count()} series have been scrapped for {season.Season}-{season.Year}"));

            return new Ovas(series.Select(Mappers.Map)
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
                    SeriesType.Ova,
                    "AniDb season scrapping failed"));
            return ExceptionError.FromException(ex, "LiveChartLibrary");
        }
    }
}