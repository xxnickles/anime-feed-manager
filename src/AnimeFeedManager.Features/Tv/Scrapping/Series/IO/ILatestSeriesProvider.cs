using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ILatestSeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary(SeasonSelector season, CancellationToken token);
}