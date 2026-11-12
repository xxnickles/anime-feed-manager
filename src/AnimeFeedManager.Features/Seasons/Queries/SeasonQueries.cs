using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Features.Seasons.Queries;

public static class SeasonQueries
{
    public static Task<Result<ImmutableArray<SeriesSeason>, DomainError>> GetLast4Seasons(Latest4SeasonsGetter seasonsGetter,
        CancellationToken cancellationToken) =>
        seasonsGetter(cancellationToken);
    
    public static Task<Result<SeasonStorageData, DomainError>> GetLatestSeason(LatestSeasonGetter seasonsGetter,
        CancellationToken cancellationToken) => seasonsGetter(cancellationToken);
}