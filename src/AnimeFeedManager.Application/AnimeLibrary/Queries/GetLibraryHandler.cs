using System.Collections.Immutable;
using AnimeFeedManager.Application.Shared.Mappers;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public sealed record LibraryForStorage(
    ImmutableList<AnimeInfoStorage> Animes,
    ImmutableList<BlobImageInfoEvent> Images,
    SeasonInfoDto Season
);

public sealed record GetLibraryQry(ImmutableList<string> feedTitles) : IRequest<Either<DomainError, LibraryForStorage>>;

public class GetLibraryHandler : IRequestHandler<GetLibraryQry, Either<DomainError, LibraryForStorage>>
{
    private readonly ILibraryProvider _libraryProvider;

    public GetLibraryHandler(ILibraryProvider libraryProvider)
    {
        _libraryProvider = libraryProvider;
    }


    public Task<Either<DomainError, LibraryForStorage>> Handle(GetLibraryQry request,
        CancellationToken cancellationToken)
    {
        return _libraryProvider.GetLibrary(request.feedTitles).MapAsync(Map);
    }

    private static LibraryForStorage Map(
        (ImmutableList<AnimeInfo> Series, ImmutableList<ImageInformation> Images) source)
    {
        var t = new LibraryForStorage(
            AnimeInfoMappers.ProjectToStorageModel(source.Series),
            Map(source.Images),
            source.Images.First().SeasonInfo
        );
        return t;
    }

    private static ImmutableList<BlobImageInfoEvent> Map(ImmutableList<ImageInformation> source)
    {
        return source.ConvertAll(Map);
    }

    private static BlobImageInfoEvent Map(ImageInformation source)
    {
        var partition = IdHelpers.GenerateAnimePartitionKey(source.SeasonInfo.Season, (ushort) source.SeasonInfo.Year);
        var directory = $"{source.SeasonInfo.Year}/{source.SeasonInfo.Season}";
        return new BlobImageInfoEvent(
            partition,
            source.Id,
            directory,
            source.Name,
            source.Link ?? string.Empty
        );
    }
}