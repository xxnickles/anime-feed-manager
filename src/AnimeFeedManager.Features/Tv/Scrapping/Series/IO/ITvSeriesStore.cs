using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ITvSeriesStore
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token);
}