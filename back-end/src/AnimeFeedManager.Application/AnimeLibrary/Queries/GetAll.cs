using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public class GetAll : Record<GetExternalLibrary>, IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
        
}