using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Feed.Commands;

public record AddProcessedTitleCmd(ProcessedTitlesStorage Entity): IRequest<Either<DomainError, Unit>>;


public class AddProcessedTitleHandler : IRequestHandler<AddProcessedTitleCmd, Either<DomainError, Unit>>
{
    private readonly IProcessedTitlesRepository _repository;

    public AddProcessedTitleHandler(IProcessedTitlesRepository repository)
    {
        _repository = repository;
    }
    
    public Task<Either<DomainError, Unit>> Handle(AddProcessedTitleCmd request, CancellationToken cancellationToken)
    {
        return _repository.Merge(request.Entity);
    }
}
