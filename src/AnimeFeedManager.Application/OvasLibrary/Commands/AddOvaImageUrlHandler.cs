namespace AnimeFeedManager.Application.OvasLibrary.Commands;

public record AddOvaImageUrlCmd(ImageStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddOvaImageUrlHandler : MediatR.IRequestHandler<AddOvaImageUrlCmd, Either<DomainError, Unit>>
{
    private readonly IOvasRepository _ovasRepository;

    public AddOvaImageUrlHandler(IOvasRepository ovasRepository) =>
        _ovasRepository = ovasRepository;


    public Task<Either<DomainError, Unit>> Handle(AddOvaImageUrlCmd request, CancellationToken cancellationToken)
    {
        return _ovasRepository.AddImageUrl(request.Entity);
    }
}