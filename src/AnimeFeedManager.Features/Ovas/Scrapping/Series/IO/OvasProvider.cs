using System.Diagnostics;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;

public interface IOvasProvider
{
    Task<Either<DomainError, OvasCollection>> GetLibrary(SeasonSelector season, CancellationToken token);
}

public sealed class OvasProvider(
    IDomainPostman domainPostman,
    PuppeteerOptions puppeteerOptions)
    : IOvasProvider
{
    public async Task<Either<DomainError, OvasCollection>> GetLibrary(SeasonSelector season, CancellationToken token)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(CreateScrappingLink(season), puppeteerOptions);

            return await domainPostman.SendMessage(new SeasonProcessNotification(
                    TargetAudience.Admins,
                    NotificationType.Information,
                    new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year, season.IsLatest()),
                    SeriesType.Ova,
                    $"{series.Count()} ovas have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                Box.SeasonProcessNotifications,
                token).MapAsync(_ => new OvasCollection(series.Select(MapInfo)
                    .ToImmutableList(),
                series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Ova))
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
                        "AniDb ovas season scrapping failed"),
                    Box.SeasonProcessNotifications,
                    token)
                .BindAsync(_ => Left<DomainError, OvasCollection>(ExceptionError.FromException(ex)));
        }
    }

    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.ova=1&type.tvspecial=1&type.web=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season.ToAlternativeString()}/?type.ova=1&type.tvspecial=1&type.web=1",
            _ => throw new UnreachableException()
        };
    }


    private static OvaStorage MapInfo(SeriesContainer container)
    {
        var seasonInfo = MapSeasonInfo(container.SeasonInfo);
        var year = seasonInfo.Year.Value;

        return new OvaStorage
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