using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Backend.Functions.Scrapping;

internal static class ScrappingUtils
{
    internal static async Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapLibraryRequest request, CancellationToken token = default)
    {
        try
        {
            await domainPostman.SendMessage(request, Box.LibraryScrapEvents, token);
            return unit;

        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}