namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

public interface ITitlesStore
{
    public Task<Either<DomainError, Unit>> UpdateTitles(IEnumerable<string> titles, CancellationToken token);
}