using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Seasons.Queries;

public class GetAvailableSeasonsHandler : IRequestHandler<GetAvailableSeasons, Either<DomainError, ImmutableList<SeasonInformation>>>
{
    private readonly ISeasonRepository _seasonRepository;

    public GetAvailableSeasonsHandler(ISeasonRepository seasonRepository) => _seasonRepository = seasonRepository;


    public Task<Either<DomainError, ImmutableList<SeasonInformation>>> Handle(GetAvailableSeasons request, CancellationToken cancellationToken)
    {
        return Fetch();
    }

    private Task<Either<DomainError, ImmutableList<SeasonInformation>>> Fetch()
    {
        return _seasonRepository.GetAvailableSeasons()
            .MapAsync(Mapper.Project);
    }
}