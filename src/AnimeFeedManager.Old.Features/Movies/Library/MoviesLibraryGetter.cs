using System.Text.Json;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Validators;
using AnimeFeedManager.Old.Features.Movies.Library.IO;
using AnimeFeedManager.Old.Features.Movies.Library.Types;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;
using SeriesFeedLinksContext = AnimeFeedManager.Old.Common.Domain.Types.SeriesFeedLinksContext;

namespace AnimeFeedManager.Old.Features.Movies.Library;

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