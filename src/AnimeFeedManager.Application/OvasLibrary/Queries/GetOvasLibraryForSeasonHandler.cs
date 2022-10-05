using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.OvasLibrary.Queries;

public sealed record GetOvasLibraryForSeasonQry
    (SeasonInfoDto Season) : IRequest<Either<DomainError, OvasLibraryForStorage>>;

public class
    GetOvasLibraryForSeasonHandler : IRequestHandler<GetOvasLibraryForSeasonQry,
        Either<DomainError, OvasLibraryForStorage>>
{
    private readonly IOvasProvider _ovasProvider;

    public GetOvasLibraryForSeasonHandler(IOvasProvider ovasProvider)
    {
        _ovasProvider = ovasProvider;
    }

    public Task<Either<DomainError, OvasLibraryForStorage>> Handle(GetOvasLibraryForSeasonQry request,
        CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToSeasonInformation(request.Season)
            .BindAsync(_ovasProvider.GetLibrary)
            .MapAsync(Mappers.Map);
    }
}