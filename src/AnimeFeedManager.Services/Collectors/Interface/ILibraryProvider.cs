using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Services.Collectors.Interface;

public interface ILibraryProvider
{
    Task<Either<DomainError, (ImmutableList<AnimeInfo> Series, ImmutableList<ImageInformation> Images)>> GetLibrary(ImmutableList<string> feedTitles);
}