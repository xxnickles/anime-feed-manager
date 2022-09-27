namespace AnimeFeedManager.Application.OvasLibrary.Commands;

public record MergeOvaCmd(OvaStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeOvaHandler : MediatR.IRequestHandler<MergeOvaCmd, Either<DomainError, Unit>>
{
    private readonly IOvasRepository _ovasRepository;

    public MergeOvaHandler(IOvasRepository ovasRepository) =>
        _ovasRepository = ovasRepository;

    public Task<Either<DomainError, Unit>> Handle(MergeOvaCmd request, CancellationToken cancellationToken)
    {
        return _ovasRepository.Merge(request.Entity);
    }
}