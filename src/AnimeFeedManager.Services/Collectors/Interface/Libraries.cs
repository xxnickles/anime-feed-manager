using System.Collections.Immutable;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface ITvSeriesProvider
{
    Task<Either<DomainError, TvSeries>> GetLibrary(ImmutableList<string> feedTitles);
}

public interface IOvasProvider
{
    Task<Either<DomainError, Ovas>> GetLibrary();
    Task<Either<DomainError, Ovas>> GetLibrary(SeasonInformation seasonInformation);
}

public interface IMoviesProvider
{
    Task<Either<DomainError, Movies>> GetLibrary();
    Task<Either<DomainError, Movies>> GetLibrary(SeasonInformation seasonInformation);
}