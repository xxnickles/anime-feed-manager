using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Tv.Library.IO;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Library;

public sealed class TvLibraryGetter
{
    private readonly ITvSeasonalLibrary _seasonalLibrary;

    public TvLibraryGetter(ITvSeasonalLibrary seasonalLibrary)
    {
        _seasonalLibrary = seasonalLibrary;
    }

    public Task<Either<DomainError, SeasonCollection>> GetForSeason(string season, ushort year, CancellationToken token = default)
    {
        return SeasonValidators.Validate(season, year)
            .BindAsync(param => _seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
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
                        new Common.Dto.Feed(!string.IsNullOrEmpty(a?.FeedTitle), a?.Completed ?? false,
                            a?.FeedTitle ?? string.Empty)
                    ))
                .ToArray());
    }
}