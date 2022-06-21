using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;
using System.Collections.Generic;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public class GetAll : Record<GetExternalLibrary>, IRequest<Either<DomainError, IEnumerable<AnimeInfoStorage>>>
{
        
}