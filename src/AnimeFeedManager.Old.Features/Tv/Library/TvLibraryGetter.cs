﻿using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Tv.Library.IO;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Library;

public sealed class TvLibraryGetter(ITvSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, SeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(animes => Project(year, season, animes));
    }


    private static SeasonCollection Project(ushort year, string season,
        IEnumerable<AnimeInfoWithImageStorage> animeInfos)
    {
        return new SeasonCollection(year, season,
            animeInfos.Select(a =>
                    new FeedAnime(
                        a.RowKey ?? string.Empty,
                        a.PartitionKey ?? string.Empty,
                        a.Title ?? "Not Available",
                        a.Synopsis ?? "Not Available",
                        a.ImageUrl,
                        a.Status is null ? SeriesStatus.NotAvailable : (SeriesStatus)a.Status,
                        new FeedData(a.Status ?? SeriesStatus.NotAvailable,
                            a?.FeedTitle ?? string.Empty)
                    ))
                .ToArray());
    }
}