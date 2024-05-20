using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Library.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Library;

public sealed class MoviesLibraryGetter(IMoviesSeasonalLibrary seasonalLibrary)
{
    public Task<Either<DomainError, ShortSeasonCollection>> GetForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(movies => Project(year, season, movies));
    }

    public Task<Either<DomainError, ImmutableList<MovieLibrary>>> GetFeedForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(ovas => ovas.ConvertAll(Project));
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

    private static MovieLibrary Project(MovieStorage movieStorage)
    {
        var id = movieStorage.RowKey ?? string.Empty;
        var season = movieStorage.PartitionKey ?? string.Empty;
        var title = movieStorage.Title ?? "Not Available";
        var synopsis = movieStorage.Synopsis ?? "Not Available";
        var imageUrl = movieStorage.ImageUrl;
        var airDate = movieStorage?.Date;

        return new MovieLibrary(
            id,
            season,
            title,
            synopsis,
            imageUrl,
            airDate,
            movieStorage?.FeedInfo is not null
                ? JsonSerializer.Deserialize(movieStorage.FeedInfo,
                    SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}