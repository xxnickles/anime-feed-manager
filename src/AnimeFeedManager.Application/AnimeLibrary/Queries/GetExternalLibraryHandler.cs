using System.Collections.Immutable;
using AnimeFeedManager.Application.Shared.Mappers;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public sealed record GetExternalLibraryQry: IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>;

public class GetExternalLibraryHandler : IRequestHandler<GetExternalLibraryQry, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IExternalLibraryProvider _externalLibraryProvider;

    public GetExternalLibraryHandler(IExternalLibraryProvider externalLibraryProvider) => _externalLibraryProvider = externalLibraryProvider;
        
    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(GetExternalLibraryQry request, CancellationToken cancellationToken) => 
        _externalLibraryProvider.GetLibrary().MapAsync(AnimeInfoMappers.ProjectToStorageModel);
}