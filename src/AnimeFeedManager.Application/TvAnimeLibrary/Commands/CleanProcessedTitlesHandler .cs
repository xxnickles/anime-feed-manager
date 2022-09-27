namespace AnimeFeedManager.Application.TvAnimeLibrary.Commands;

public record CleanProcessedTitlesCmd : MediatR.IRequest<Either<DomainError, Unit>>;

public class CleanProcessedTitlesHandler : MediatR.IRequestHandler<CleanProcessedTitlesCmd, Either<DomainError, Unit>>
{
    private readonly IProcessedTitlesRepository _processedTitles;

    public CleanProcessedTitlesHandler(IProcessedTitlesRepository processedTitles)
    {
        _processedTitles = processedTitles;
    }

    public Task<Either<DomainError, Unit>> Handle(CleanProcessedTitlesCmd request, CancellationToken cancellationToken)
    {
        return _processedTitles.RemoveExpired();
    }
}