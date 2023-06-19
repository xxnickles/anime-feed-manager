using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public interface ITvSeriesStore
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series);
}