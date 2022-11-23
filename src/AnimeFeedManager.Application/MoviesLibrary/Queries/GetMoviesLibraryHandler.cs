using System.Collections.Immutable;
using AnimeFeedManager.Application.Mappers;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.MoviesLibrary.Queries;

public sealed record GetMoviesLibraryQry() : IRequest<Either<DomainError, MoviesLibraryForStorage>>;

public class GetMoviesLibraryHandler : IRequestHandler<GetMoviesLibraryQry, Either<DomainError, MoviesLibraryForStorage>>
{
    private readonly IMoviesProvider _moviesProvider;

    public GetMoviesLibraryHandler(IMoviesProvider moviesProvider)
    {
        _moviesProvider = moviesProvider;
    }

    public Task<Either<DomainError, MoviesLibraryForStorage>> Handle(GetMoviesLibraryQry request,
        CancellationToken cancellationToken)
    {
        return _moviesProvider.GetLibrary().MapAsync(Map);
    }

    private static MoviesLibraryForStorage Map(Movies source)
    {
        return new MoviesLibraryForStorage(
            MoviesMappers.ProjectToStorageModel(source.SeriesList),
            Map(source.Images),
            source.Images.First().SeasonInfo.Map()
        );
    }

    private static ImmutableList<BlobImageInfoEvent> Map(ImmutableList<ImageInformation> source)
    {
        return source.ConvertAll(Map);
    }

    private static BlobImageInfoEvent Map(ImageInformation source)
    {
        var season = source.SeasonInfo.Map();
        var partition = IdHelpers.GenerateAnimePartitionKey(season.Season, (ushort) season.Year);
        var directory = $"{season.Year}/{season.Season}";
        return new BlobImageInfoEvent(
            partition,
            source.Id,
            directory,
            source.Name,
            source.Link ?? string.Empty,
            SeriesType.Movie
        );
    }
}