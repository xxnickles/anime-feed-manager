using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Old.Web.Features.Common;

internal static class ScrappingUtils
{
    internal static Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapLibraryRequest request, CancellationToken token = default)
    {
        return domainPostman.SendMessage(request, token);
    }

    internal static Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapTvTilesRequest request, CancellationToken token)
    {
        return domainPostman.SendMessage(request, token);
    }
}