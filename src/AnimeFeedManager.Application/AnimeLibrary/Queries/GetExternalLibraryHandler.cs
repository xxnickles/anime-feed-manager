using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Shared.Mappers;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public sealed record GetExternalLibraryQry: IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>;

public class GetExternalLibraryHandler : IRequestHandler<GetExternalLibraryQry, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IExternalLibraryProvider _externalLibraryProvider;

    public GetExternalLibraryHandler(IExternalLibraryProvider externalLibraryProvider) => _externalLibraryProvider = externalLibraryProvider;
        
    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(GetExternalLibraryQry request, CancellationToken cancellationToken) => 
        _externalLibraryProvider.GetLibrary().MapAsync(AnimeInfoMappers.ProjectToStorageModelWithEtag);
}