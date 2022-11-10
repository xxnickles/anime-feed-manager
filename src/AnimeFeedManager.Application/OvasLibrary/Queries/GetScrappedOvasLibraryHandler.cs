using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.OvasLibrary.Queries;

public sealed record GetScrappedOvasLibraryQry
    (SeasonInfoDto Season) : IRequest<Either<DomainError, OvasLibraryForStorage>>;

public sealed class GetScrappedOvasLibraryHandler : IRequestHandler<GetScrappedOvasLibraryQry,
    Either<DomainError, OvasLibraryForStorage>>
{
    private readonly IOvasProvider _ovasProvider;

    public GetScrappedOvasLibraryHandler(IOvasProvider ovasProvider)
    {
        _ovasProvider = ovasProvider;
    }

    public Task<Either<DomainError, OvasLibraryForStorage>> Handle(GetScrappedOvasLibraryQry request,
        CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToSeasonInformation(request.Season)
            .BindAsync(_ovasProvider.GetLibrary)
            .MapAsync(Mappers.Map);
    }
}