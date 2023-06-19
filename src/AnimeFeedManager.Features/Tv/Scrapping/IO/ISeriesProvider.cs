using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public interface ISeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary();
}