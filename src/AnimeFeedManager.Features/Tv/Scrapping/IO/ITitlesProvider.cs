namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public interface ITitlesProvider
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
}