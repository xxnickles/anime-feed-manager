using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface IExternalLibraryProvider
{
    Task<Either<DomainError, ImmutableList<AnimeInfo>>> GetLibrary();
}

public interface ILibraryProvider
{
    Task<Either<DomainError, (ImmutableList<AnimeInfo> Series, ImmutableList<ImageInformation> Titles)>> GetLibrary();
}