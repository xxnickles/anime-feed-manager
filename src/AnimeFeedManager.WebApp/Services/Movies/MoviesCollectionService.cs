using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Movies;

public interface IMoviesCollectionService
{
    public Task<ShortSeasonCollection> GetSeasonLibrary(SeasonInfoDto season, CancellationToken cancellationToken = default);
}

public sealed class MoviesCollectionService : IMoviesCollectionService
{
    private readonly HttpClient _httpClient;

    public MoviesCollectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<ShortSeasonCollection> GetSeasonLibrary(SeasonInfoDto season, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Movies/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject<ShortSeasonCollection>(new EmptyShortSeasonCollection());
    }
}