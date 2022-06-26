using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public record GetAllQry :  IRequest<Either<DomainError, ImmutableList<AnimeInfoStorage>>>;

public class GetAllHandler: IRequestHandler<GetAllQry, Either<DomainError, ImmutableList<AnimeInfoStorage>>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public GetAllHandler(IAnimeInfoRepository animeInfoRepository)
    {
        _animeInfoRepository = animeInfoRepository;
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoStorage>>> Handle(GetAllQry request, CancellationToken cancellationToken)
    {
        return  _animeInfoRepository.GetAll();
    }
}