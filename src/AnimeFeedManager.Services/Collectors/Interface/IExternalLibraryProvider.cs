using System.Collections.Immutable;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface IExternalLibraryProvider
{
    Task<Either<DomainError, ImmutableList<AnimeInfo>>> GetLibrary();
}