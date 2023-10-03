using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public class ScrapSeasonTitles
{
    private readonly ITitlesProvider _titlesProvider;
    private readonly IDomainPostman _domainPostman;

    public ScrapSeasonTitles(ITitlesProvider titlesProvider, IDomainPostman domainPostman)
    {
        _titlesProvider = titlesProvider;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, Unit>> Scrap(CancellationToken token = default)
    {
       return _titlesProvider.GetTitles()
           .BindAsync(titles => _domainPostman.SendMessage(new UpdateSeasonTitlesRequest(titles), Box.SeasonTitlesProcess,  token));
    }
}