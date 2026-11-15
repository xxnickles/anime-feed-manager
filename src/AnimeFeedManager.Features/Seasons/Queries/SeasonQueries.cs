namespace AnimeFeedManager.Features.Seasons.Queries;

[Widens]
public partial union LatestSeasonError(StorageError, NoLatestSeason);

public static class SeasonQueries
{
    public static Task<Result<ImmutableArray<SeriesSeason>, StorageError>> GetLast4Seasons(Latest4SeasonsGetter seasonsGetter,
        CancellationToken cancellationToken) =>
        seasonsGetter(cancellationToken);

    /// <summary>The featured season as a series season; a stored season that doesn't parse is <see cref="StorageError.InvalidRecord"/>.</summary>
    public static Task<Result<SeriesSeason, LatestSeasonError>> GetLatestSeason(LatestSeasonGetter seasonGetter,
        CancellationToken cancellationToken) =>
        seasonGetter(cancellationToken)
            .MapError(error => (LatestSeasonError)error)
            .Bind(stored => stored.AsFeaturedSeason())
            .Bind(AsSeriesSeason);

    private static Result<SeriesSeason, StorageError> AsSeriesSeason(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, season.Latest).ParseAsSeriesSeason()
            .MapError(StorageError (fields) => new StorageError.InvalidRecord(typeof(SeasonStorage), fields.Errors));
}
