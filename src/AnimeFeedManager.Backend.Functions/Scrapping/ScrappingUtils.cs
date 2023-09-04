﻿using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Backend.Functions.Scrapping;

internal static class ScrappingUtils
{
    internal static Task<Either<DomainError, Unit>> CreateScrapingEvent(this IDomainPostman domainPostman,
        ScrapLibraryRequest request, CancellationToken token = default)
    {
        return domainPostman.SendMessage(request, Box.LibraryScrapEvents, token);
    }
}