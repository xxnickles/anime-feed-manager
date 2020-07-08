using System.Collections.Immutable;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands
{
    public class UpdateStatus : Record<GetExternalLibrary>, IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>
    {
        
    }
}