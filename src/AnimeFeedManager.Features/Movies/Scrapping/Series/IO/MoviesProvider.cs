using System.Diagnostics;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Series.IO;

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
                    token)
                .BindAsync(_ => GetSeasonInformation(jsonSeason))
                .MapAsync(seasonInfo => new MoviesCollection(
                    seasonInfo,
                    series.Select(MapInfo).ToImmutableList(),
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
                    token)
                .BindAsync(_ => Left<DomainError, MoviesCollection>(ExceptionError.FromException(ex)));
        }
    }
    
    private static Either<DomainError, SeasonInformation> GetSeasonInformation(JsonSeasonInfo jsonSeasonInfo)
    {
        return SeasonValidators.Parse(jsonSeasonInfo.Season, (ushort)jsonSeasonInfo.Year)
            .Map(data => new SeasonInformation(data.Season, data.Year));
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
        var (season, year) = MapSeasonInfo(container.SeasonInfo);
        var date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime();
        return new MovieStorage
        {
            RowKey = container.Id,
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(season, year),
            Title = container.Title,
            Synopsis = container.Synopsys,
            Date = date,
            Season = season.Value,
            Year = year,
            Status = date is not null ? ShortSeriesStatus.NotProcessed : ShortSeriesStatus.NotAvailable
        };
    }

    private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}