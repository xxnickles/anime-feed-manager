using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using LanguageExt;

namespace AnimeFeedManager.Web.Features.Common;

internal static class ScrappingUtils
{
    internal static Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapLibraryRequest request, CancellationToken token = default)
    {
        return domainPostman.SendMessage(request, Box.LibraryScrapEvents, token);
    }

    internal static Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapTvTilesRequest request, CancellationToken token)
    {
        return domainPostman.SendMessage(request, Box.TvTitlesScrapEvents, token);
    }
}