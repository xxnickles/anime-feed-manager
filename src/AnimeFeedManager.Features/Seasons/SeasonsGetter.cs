using AnimeFeedManager.Features.Seasons.IO;
using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons;

public sealed class SeasonsGetter
{
    private readonly ISeasonsGetter _seasonsGetter;

    public SeasonsGetter(ISeasonsGetter seasonsGetter)
    {
        _seasonsGetter = seasonsGetter;
    }
    
    public Task<Either<DomainError,ImmutableList<SimpleSeasonInfo>>> GetAvailable()
    {
       return _seasonsGetter.GetAvailableSeasons()
            .MapAsync(seasons => 
                seasons.ConvertAll(s => s.ToWrapper())
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Season)
                    .Select(s => s.ToSimpleSeason())
                    .ToImmutableList());
    }
    
}