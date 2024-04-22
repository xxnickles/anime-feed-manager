using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Web.Features.Common;

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