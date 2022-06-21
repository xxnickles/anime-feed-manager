using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public class GetAllHandler: IRequestHandler<GetAll, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public GetAllHandler(IAnimeInfoRepository animeInfoRepository)
    {
        _animeInfoRepository = animeInfoRepository;
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        return  _animeInfoRepository.GetAll();
    }
}