using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands;

public class MergeAnimeInfo : IRequest<Either<DomainError, LanguageExt.Unit>>
{
    public AnimeInfoStorage Entity { get; }

    public MergeAnimeInfo(AnimeInfoStorage entity)
    {
        Entity = entity;
    }
}