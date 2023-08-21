using AnimeFeedManager.Features.Domain.Validators;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Library;

public sealed class MoviesLibraryGetter
{
    private readonly IMoviesSeasonalLibrary _seasonalLibrary;

    public MoviesLibraryGetter(IMoviesSeasonalLibrary seasonalLibrary)
    {
        _seasonalLibrary = seasonalLibrary;
    }

    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Validate(season, year)
            .BindAsync(param => _seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
            .MapAsync(movies => Project(year, season, movies));
    }

    private static ShortSeasonCollection Project(ushort year, string season,
        IEnumerable<MovieStorage> movies)
    {
        return new ShortSeasonCollection(year, season,
            movies.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty,
                        a?.Title ?? "Not Available",
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl,
                        a?.Date))
                .ToArray());
    }
}