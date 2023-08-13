using System.Diagnostics;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage;
using NotificationType = AnimeFeedManager.Features.Domain.Notifications.NotificationType;
using TargetAudience = AnimeFeedManager.Features.Domain.Notifications.TargetAudience;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public sealed class SeriesProvider : ISeriesProvider
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

    public async Task<Either<DomainError, TvSeries>> GetLibrary(SeasonSelector season, CancellationToken token)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(CreateScrappingLink(season), _puppeteerOptions);

            await _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year, season.IsLatest()),
                    SeriesType.Tv,
                    $"{series.Count()} series have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                Boxes.SeasonProcessNotifications,
                token);

            return new TvSeries(series.Select(MapInfo)
                    .ToImmutableList(),
                series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Tv))
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
                    "AniDb season scrapping failed"),
                Boxes.SeasonProcessNotifications,
                token);
            return ExceptionError.FromException(ex);
        }
    }

    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.tvseries=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season}/?do=calendar&h=1&type.tvseries=1",
            _ => throw new UnreachableException()
        };
    }

    private static AnimeInfoStorage MapInfo(SeriesContainer container)
    {
        var seasonInfo = MapSeasonInfo(container.SeasonInfo);
        var year = seasonInfo.Year.Value.UnpackOption((ushort)0);

        return new AnimeInfoStorage
        {
            RowKey = container.Id,
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(seasonInfo.Season, year),
            Title = container.Title,
            Synopsis = container.Synopsys,
            FeedTitle = string.Empty,
            Date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime(),
            Completed = false,
            Season = seasonInfo.Season.Value,
            Year = year
        };
    }

    private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}