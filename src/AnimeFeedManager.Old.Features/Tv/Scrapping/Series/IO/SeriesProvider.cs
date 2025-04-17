using System.Diagnostics;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Domain.Validators;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.AniDb;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Old.Features.Tv.Types;
using NotificationType = AnimeFeedManager.Old.Common.Domain.Notifications.Base.NotificationType;
using TargetAudience = AnimeFeedManager.Old.Common.Domain.Notifications.Base.TargetAudience;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.IO;

public interface ISeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary(SeasonSelector season, CancellationToken token);
}

public sealed class SeriesProvider(
    IDomainPostman domainPostman,
    PuppeteerOptions puppeteerOptions,
    ITvSeriesStatusProvider seriesStatusProvider,
    TimeProvider timeProvider)
    : ISeriesProvider
{
    private readonly record struct ProcessData(
        ImmutableList<TvSeriesStatus> CompletedSeries,
        SeasonInformation SeasonInfo,
        bool IsOldSeason);

    public async Task<Either<DomainError, TvSeries>> GetLibrary(SeasonSelector season, CancellationToken token)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(CreateScrappingLink(season), puppeteerOptions);

            return await domainPostman.SendMessage(new SeasonProcessNotification(
                        TargetAudience.Admins,
                        NotificationType.Information,
                        new SimpleSeasonInfo(jsonSeason.Season, jsonSeason.Year, season.IsLatest()),
                        SeriesType.Tv,
                        $"{series.Count()} series have been scrapped for {jsonSeason.Season}-{jsonSeason.Year}"),
                    token)
                .BindAsync(_ => SeasonValidators.Parse(jsonSeason.Season, (ushort)jsonSeason.Year))
                .BindAsync(seasonInfo => PrepareSeriesData(seasonInfo.Season, seasonInfo.Year, token))
                .MapAsync(processData => new TvSeries(series
                        .Select(s => MapInfo(s, processData.CompletedSeries, processData.SeasonInfo,
                            processData.IsOldSeason))
                        .ToImmutableList(),
                    series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                        .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer, SeriesType.Tv))
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
                        "AniDb season scrapping failed"),
                    token)
                .BindAsync(_ => Left<DomainError, TvSeries>(ExceptionError.FromException(ex)));
        }
    }

    private Task<Either<DomainError, ProcessData>> PrepareSeriesData(Season season, Year year, CancellationToken token)
    {
        var seasonInfo = new SeasonInformation(season, year);
        return seriesStatusProvider.GetSeasonSeriesCurrentStatus(seasonInfo.Season, seasonInfo.Year, token)
            .MapAsync(s => new ProcessData(s, seasonInfo, IsOldSeason(seasonInfo)));
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

    private static AnimeInfoStorage MapInfo(
        SeriesContainer container,
        ImmutableList<TvSeriesStatus> statusList,
        SeasonInformation seasonInformation,
        bool isOldSeason)
    {
        var year = seasonInformation.Year.Value;
        var currentStatus = statusList.FirstOrDefault(s => container.Title == s.Title);

        var status = SeriesStatus.NotAvailable;

        if (currentStatus != default)
        {
            status = currentStatus.Status;
        }
        else if (isOldSeason)
        {
            status = SeriesStatus.Completed;
        }

        return new AnimeInfoStorage
        {
            RowKey = container.Id,
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(seasonInformation.Season, year),
            Title = container.Title,
            Synopsis = container.Synopsys,
            FeedTitle = string.Empty,
            Date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime(),
            Status = status,
            Season = seasonInformation.Season.Value,
            Year = year
        };
    }

    private bool IsOldSeason(SeasonInformation seasonInformation)
    {
        var reference = timeProvider.GetUtcNow().AddMonths(-6);
        var referenceSeason = reference.Month switch
        {
            < 4 => Season.Winter,
            < 7 => Season.Spring,
            < 10 => Season.Summer,
            _ => Season.Fall
        };

        var referenceYear = Year.FromNumber(reference.Year);
        if (seasonInformation.Year > referenceYear)
        {
            return false;
        }

        if (seasonInformation.Year == referenceYear)
        {
            return referenceSeason > seasonInformation.Season;
        }
        return true;
    }
}