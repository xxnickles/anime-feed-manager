using AnimeFeedManager.Features.Movies.Scrapping.Types;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public interface IMoviesProvider
{
    Task<Either<DomainError, MoviesCollection>> GetLibrary(SeasonSelector season, CancellationToken token);
}