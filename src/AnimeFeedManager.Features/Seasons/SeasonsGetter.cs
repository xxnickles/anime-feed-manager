using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Seasons.IO;
using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons;

public sealed class SeasonsGetter(ISeasonsGetter seasonsGetter)
{
    public Task<Either<DomainError, ImmutableList<SimpleSeasonInfo>>> GetAvailable(CancellationToken token = default)
    {
        return seasonsGetter.GetAvailableSeasons(token)
            .MapAsync(seasons =>
                seasons.ConvertAll(s => s.ToWrapper())
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Season)
                    .Select(s => s.ToSimpleSeason())
                    .ToImmutableList());
    }

    public Task<Either<DomainError, ImmutableList<SeasonWrapper>>> GetLastSeasons(CancellationToken token = default)
    {
        return seasonsGetter.GetLastFourSeasons(token)
            .MapAsync(seasons => seasons.ConvertAll(s => s.ToWrapper())
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Season)
                .ToImmutableList()
            );
    }

    public Task<Either<DomainError, SeasonWrapper>> GetCurrentSeason(CancellationToken token = default)
    {
        return seasonsGetter.GetCurrentSeason(token).MapAsync(season => season.ToWrapper());
    }
}