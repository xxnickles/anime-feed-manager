using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Feed.Commands
{
    public class CleanProcessedTitlesHandler : IRequestHandler<CleanProcessedTitles, Either<DomainError, LanguageExt.Unit>>
    {
        private readonly IProcessedTitlesRepository _processedTitles;

        public CleanProcessedTitlesHandler(IProcessedTitlesRepository processedTitles)
        {
            _processedTitles = processedTitles;
        }

        public Task<Either<DomainError, Unit>> Handle(CleanProcessedTitles request, CancellationToken cancellationToken)
        {
            return _processedTitles.RemoveExpired();
        }
    }
}