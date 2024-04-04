using System.Diagnostics;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public interface IMoviesProvider
{
    Task<Either<DomainError, MoviesCollection>> GetLibrary(SeasonSelector season, CancellationToken token);
}

public sealed class MoviesProvider(
    IDomainPostman domainPostman,
    PuppeteerOptions puppeteerOptions)
    : IMoviesProvider
{
    public async Task<Either<DomainError, MoviesCollection>> GetLibrary(SeasonSelector season, CancellationToken token)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(CreateScrappingLink(season), puppeteerOptions);

            return await domainPostman.SendMessage(new SeasonProcessNotification(
                        TargetAudience.Admins,
                        NotificationType.Information,
                        new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year, season.IsLatest()),
                        SeriesType.Movie,
                        $"{series.Count()} movies have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                    Box.SeasonProcessNotifications,
                    token)
                .MapAsync(_ => new MoviesCollection(series.Select(MapInfo)
                        .ToImmutableList(),
                    series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                        .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Movie))
                        .ToImmutableList()));
        }
        catch (Exception ex)
        {
            return await domainPostman.SendMessage(
                    new SeasonProcessNotification(
                        TargetAudience.Admins,
                        NotificationType.Error,
                        new NullSimpleSeasonInfo(),
                        SeriesType.Tv,
                        "AniDb movies season scrapping failed"),
                    Box.SeasonProcessNotifications,
                    token)
                .BindAsync(_ => Left<DomainError, MoviesCollection>(ExceptionError.FromException(ex)));
        }
    }
    
    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.movie=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season.ToAlternativeString()}/?type.movie=1",
            _ => throw new UnreachableException()
        };
    }

    private static MovieStorage MapInfo(SeriesContainer container)
    {
        var seasonInfo = MapSeasonInfo(container.SeasonInfo);
        var year = seasonInfo.Year;

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