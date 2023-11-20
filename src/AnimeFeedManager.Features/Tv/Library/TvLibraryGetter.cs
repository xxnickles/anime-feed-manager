using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Tv.Library.IO;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Library;

public sealed class TvLibraryGetter(ITvSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, SeasonCollection>> GetForSeason(string season, ushort year, CancellationToken token = default)
    {
        return SeasonValidators.Validate(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
            .MapAsync(animes => Project(year, season, animes));
    }


    private static SeasonCollection Project(ushort year, string season,
        IEnumerable<AnimeInfoWithImageStorage> animeInfos)
    {
        return new SeasonCollection(year, season,
            animeInfos.Select(a =>
                    new FeedAnime(
                        a.RowKey ?? string.Empty,
                        a?.Title ?? "Not Available",
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl,
                        new FeedData(!string.IsNullOrEmpty(a?.FeedTitle), a?.Status ?? SeriesStatus.NotAvailable,
                            a?.FeedTitle ?? string.Empty)
                    ))
                .ToArray());
    }
}