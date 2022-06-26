using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Seasons.Commands;

public record MergeSeasonHandlerCmd(SeasonStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeSeasonHandler: IRequestHandler<MergeSeasonHandlerCmd, Either<DomainError, Unit>>
{
    private readonly ISeasonRepository _repository;

    public MergeSeasonHandler(ISeasonRepository repository)
    {
        _repository = repository;
    }
    
    public Task<Either<DomainError, Unit>> Handle(MergeSeasonHandlerCmd request, CancellationToken cancellationToken)
    {
        return _repository.Merge(request.Entity);
    }
}