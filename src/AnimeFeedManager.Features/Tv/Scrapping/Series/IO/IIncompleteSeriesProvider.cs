using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface IIncompleteSeriesProvider
{
    Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncompleteSeries(CancellationToken token);
}