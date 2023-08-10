using System.Diagnostics;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public sealed class MoviesProvider : IMoviesProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    public MoviesProvider(IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, MoviesCollection>> GetLibrary(SeasonSelector season, CancellationToken token)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(CreateScrappingLink(season), _puppeteerOptions);

            await _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year),
                    SeriesType.Movie,
                    $"{series.Count()} movies have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                Boxes.SeasonProcessNotifications,
                token);

            return new MoviesCollection(series.Select(MapInfo)
                    .ToImmutableList(),
                series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Ova))
                    .ToImmutableList());
        }
        catch (Exception ex)
        {
            await _domainPostman.SendMessage(
                new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSimpleSeasonInfo(),
                    SeriesType.Tv,
                    "AniDb movies season scrapping failed"),
                Boxes.SeasonProcessNotifications,
                token);
            return ExceptionError.FromException(ex);
        }
    }
    
    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.movie=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.SeasonInfo.Year}/{s.SeasonInfo.Season}/?type.movie=1",
            _ => throw new UnreachableException()
        };
    }
    
    
    private static MovieStorage MapInfo(SeriesContainer container)
    {
        var seasonInfo = MapSeasonInfo(container.SeasonInfo);
        var year = seasonInfo.Year.Value.UnpackOption((ushort)0);

        return new MovieStorage
        {
            RowKey = container.Id,
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(seasonInfo.Season, year),
            Title = container.Title,
            Synopsis = container.Synopsys,
            Date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime(),
            Season = seasonInfo.Season.Value,
            Year = year
        };
    }

    private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}