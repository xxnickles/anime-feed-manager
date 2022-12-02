using System.Collections.Immutable;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;

namespace AnimeFeedManager.Services.Collectors.AniDb;

public class MoviesProvider : IMoviesProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    public MoviesProvider(
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public Task<Either<DomainError, Movies>> GetLibrary()
    {
        return Process("https://anidb.net/anime/season/?type.movie=1");
    }

    public Task<Either<DomainError, Movies>> GetLibrary(SeasonInformation seasonInformation)
    {
        var (season,year) = seasonInformation.UnwrapForAniDb();
        return Process($"https://anidb.net/anime/season/{year}/{season}/?type.movie=1");
    }

    private async Task<Either<DomainError, Movies>> Process(string url)
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
                SeriesType.Movie,
                $"{series.Count()} series have been scrapped for {season.Season}-{season.Year}"));

            return new Movies(series.Select(Mappers.Map)
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
                    SeriesType.Movie,
                    "AniDb season scrapping failed"));
            return ExceptionError.FromException(ex, "LiveChartLibrary");
        }
    }
}