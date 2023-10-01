using System.Diagnostics;
using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Types;
using NotificationType = AnimeFeedManager.Features.Common.Domain.Notifications.Base.NotificationType;
using TargetAudience = AnimeFeedManager.Features.Common.Domain.Notifications.Base.TargetAudience;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ISeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary(SeasonSelector season, CancellationToken token);
}

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

            return await _domainPostman.SendMessage(new SeasonProcessNotification(
                        TargetAudience.Admins,
                        NotificationType.Information,
                        new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year, season.IsLatest()),
                        SeriesType.Tv,
                        $"{series.Count()} series have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                    Box.SeasonProcessNotifications,
                    token)
                .MapAsync(_ => new TvSeries(series.Select(MapInfo)
                        .ToImmutableList(),
                    series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                        .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Tv))
                        .ToImmutableList()));
        }
        catch (Exception ex)
        {
            return await _domainPostman.SendMessage(
                    new SeasonProcessNotification(
                        TargetAudience.Admins,
                        NotificationType.Error,
                        new NullSimpleSeasonInfo(),
                        SeriesType.Tv,
                        "AniDb season scrapping failed"),
                    Box.SeasonProcessNotifications,
                    token)
                .BindAsync(_ => Left<DomainError, TvSeries>(ExceptionError.FromException(ex)));
        }
    }

    private static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.tvseries=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season.ToAlternativeString()}/?do=calendar&h=1&type.tvseries=1",
            _ => throw new UnreachableException()
        };
    }

    private static AnimeInfoStorage MapInfo(SeriesContainer container)
    {
        var seasonInfo = MapSeasonInfo(container.SeasonInfo);
        var year = seasonInfo.Year.Value;

        return new AnimeInfoStorage
        {
            RowKey = container.Id,
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(seasonInfo.Season, year),
            Title = container.Title,
            Synopsis = container.Synopsys,
            FeedTitle = string.Empty,
            Date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime(),
            Status = SeriesStatus.NotAvailable,
            Season = seasonInfo.Season.Value,
            Year = year
        };
    }

    private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}