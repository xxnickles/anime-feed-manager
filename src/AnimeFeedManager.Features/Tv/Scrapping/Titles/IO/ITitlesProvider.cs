namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

public interface ITitlesProvider
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
}