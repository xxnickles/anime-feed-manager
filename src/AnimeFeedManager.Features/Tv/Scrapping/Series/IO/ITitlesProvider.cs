namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ITitlesProvider
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
}