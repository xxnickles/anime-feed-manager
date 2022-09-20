using System.Collections.Immutable;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface ITvSeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary(ImmutableList<string> feedTitles);
}