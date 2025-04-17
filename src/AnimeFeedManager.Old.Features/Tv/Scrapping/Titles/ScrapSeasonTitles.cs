using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Titles.IO;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Titles;

public class ScrapSeasonTitles(ITitlesProvider titlesProvider, IDomainPostman domainPostman)
{
    public Task<Either<DomainError, Unit>> Scrap(CancellationToken token = default)
    {
        return titlesProvider.GetTitles()
            .BindAsync(titles => domainPostman.SendMessage(new UpdateSeasonTitlesRequest(titles),   token));
    }
}