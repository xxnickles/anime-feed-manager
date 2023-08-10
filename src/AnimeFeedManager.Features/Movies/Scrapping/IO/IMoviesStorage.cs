using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public interface IMoviesStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token);
}