using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Movies;

public interface IMoviesCollectionService
{
    public Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default);
}

public sealed class MoviesCollectionService(HttpClient httpClient) : IMoviesCollectionService
{
    public async Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/movies/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject(ShortSeasonCollectionContext.Default.ShortSeasonCollection, new EmptyShortSeasonCollection());
    }
}