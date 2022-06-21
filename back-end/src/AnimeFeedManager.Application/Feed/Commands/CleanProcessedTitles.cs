using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Feed.Commands;

public class CleanProcessedTitles :  Record<CleanProcessedTitles>, IRequest<Either<DomainError, Unit>>
{
        
}