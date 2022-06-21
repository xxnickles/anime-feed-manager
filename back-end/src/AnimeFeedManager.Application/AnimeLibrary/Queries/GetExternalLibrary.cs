using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public sealed class GetExternalLibrary: Record<GetExternalLibrary>, IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
}