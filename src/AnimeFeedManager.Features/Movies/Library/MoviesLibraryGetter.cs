using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Library;

public sealed class MoviesLibraryGetter(IMoviesSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.season, param.year, token))
            .MapAsync(movies => Project(year, season, movies));
    }

    private static ShortSeasonCollection Project(ushort year, string season,
        IEnumerable<MovieStorage> movies)
    {
        return new ShortSeasonCollection(year, season,
            movies.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty,
                        a.PartitionKey ?? string.Empty,
                        a?.Title ?? "Not Available",
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl,
                        a?.Date))
                .ToArray());
    }
}