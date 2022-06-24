using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;

namespace AnimeFeedManager.Application.Feed.Commands;

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