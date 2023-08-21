using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Library.IO;

public interface IMoviesSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}