using AnimeFeedManager.Application.Shared.Mappers;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.LiveChart;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Services.Collectors.Interface;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public class GetExternalLibraryHandler : IRequestHandler<GetExternalLibrary, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IExternalLibraryProvider _externalLibraryProvider;

    public GetExternalLibraryHandler(IExternalLibraryProvider externalLibraryProvider) => _externalLibraryProvider = externalLibraryProvider;
        
    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(GetExternalLibrary request, CancellationToken cancellationToken) => 
        _externalLibraryProvider.GetLibrary().MapAsync(AnimeInfoMappers.ProjectToStorageModelWithEtag);
}