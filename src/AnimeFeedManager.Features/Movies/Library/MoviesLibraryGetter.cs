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
    public Task<Either<DomainError, ImmutableList<MovieLibrary>>> GetFeedForSeason(string season, ushort year,
        CancellationToken token = default)
    {
        return SeasonValidators.Parse(season, year)
            .BindAsync(param => seasonalLibrary.GetSeasonalLibrary(param.Season, param.Year, token))
            .MapAsync(ovas => ovas.ConvertAll(Project));
    }

    private static MovieLibrary Project(MovieStorage movieStorage)
    {
        var id = movieStorage.RowKey ?? string.Empty;
        var season = movieStorage.PartitionKey ?? string.Empty;
        var title = movieStorage.Title ?? "Not Available";
        var synopsis = movieStorage.Synopsis ?? "Not Available";
        var imageUrl = movieStorage.ImageUrl;
        var airDate = movieStorage.Date;

        return new MovieLibrary(
            id,
            season,
            title,
            synopsis,
            imageUrl,
            airDate,
            !string.IsNullOrEmpty(movieStorage.FeedInfo)
                ? JsonSerializer.Deserialize(movieStorage.FeedInfo,
                    SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}