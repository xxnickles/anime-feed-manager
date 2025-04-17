using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Features.Seasons.IO;

namespace AnimeFeedManager.Old.Features.Seasons;

public sealed class SeasonsGetter(ISeasonsGetter seasonsGetter, ILatestSeasonsGetter latestSeasonsGetter)
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

    public Task<Either<DomainError, ImmutableList<SeasonGroup>>> GroupAvailable(CancellationToken token = default)
    {
       return seasonsGetter.GetAvailableSeasons(token)
            .MapAsync(seasons =>
                seasons.ConvertAll(s => s.ToWrapper())
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Season)
                    .GroupBy(s => s.Year)
                    .Select(group => group.ToGroup(group.Key))
                    .ToImmutableList());
    }

    public Task<Either<DomainError, ImmutableList<SeasonWrapper>>> GetLastSeasons(CancellationToken token = default)
    {
        return latestSeasonsGetter.Get(token);
    }
    public Task<Either<DomainError, SeasonWrapper>> GetCurrentSeason(CancellationToken token = default)
    {
        return seasonsGetter.GetCurrentSeason(token).MapAsync(season => season.ToWrapper());
    }
}