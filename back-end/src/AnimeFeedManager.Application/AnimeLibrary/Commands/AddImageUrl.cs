using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands;

public class AddImageUrl : IRequest<Either<DomainError, LanguageExt.Unit>>
{
    public ImageStorage Entity { get; }

    public AddImageUrl(ImageStorage entity)
    {
        Entity = entity;
    }
}