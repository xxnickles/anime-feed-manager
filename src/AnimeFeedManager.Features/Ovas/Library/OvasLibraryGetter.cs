﻿using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Library.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library;

public sealed class OvasLibraryGetter(IOvasSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(ovas => Project(year, season, ovas));
    }

    public Task<Either<DomainError, ImmutableList<OvaLibrary>>> GetFeedForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(ovas => ovas.ConvertAll(Project));
    }

    private static ShortSeasonCollection Project(ushort year, string season,
        IEnumerable<OvaStorage> ovas)
    {
        return new ShortSeasonCollection(year, season,
            ovas.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty,
                        a.PartitionKey ?? string.Empty,
                        a.Title ?? "Not Available",
                        a.Synopsis ?? "Not Available",
                        a.ImageUrl,
                        a?.Date))
                .ToArray());
    }

    private static OvaLibrary Project(OvaStorage ovaStorage)
    {
        var id = ovaStorage.RowKey ?? string.Empty;
        var season = ovaStorage.PartitionKey ?? string.Empty;
        var title = ovaStorage.Title ?? "Not Available";
        var synopsis = ovaStorage.Synopsis ?? "Not Available";
        var imageUrl = ovaStorage.ImageUrl;
        var airDate = ovaStorage?.Date;


        return new OvaLibrary(
            id,
            season,
            title,
            synopsis,
            imageUrl,
            airDate,
            ovaStorage?.FeedInfo is not null
                ? JsonSerializer.Deserialize(ovaStorage.FeedInfo, SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}