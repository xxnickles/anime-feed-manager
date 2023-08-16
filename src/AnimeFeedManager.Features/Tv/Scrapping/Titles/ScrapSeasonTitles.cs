using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles;

public class ScrapSeasonTitles
{
    private readonly ITitlesProvider _titlesProvider;
    private readonly IMediator _mediator;

    public ScrapSeasonTitles(ITitlesProvider titlesProvider, IMediator mediator)
    {
        _titlesProvider = titlesProvider;
        _mediator = mediator;
    }

    public Task<Either<DomainError, Unit>> Scrap(CancellationToken token = default)
    {
       return _titlesProvider.GetTitles()
           .MapAsync(titles => _mediator.Publish(new UpdateSeasonTitles(titles), token))
           .MapAsync(_ => unit);
    }
}