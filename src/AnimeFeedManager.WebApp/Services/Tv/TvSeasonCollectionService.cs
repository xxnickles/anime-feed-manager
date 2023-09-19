using AnimeFeedManager.Features.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Tv;

public interface ITvCollectionFetcher
{
    public Task<SeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season, CancellationToken cancellationToken = default);
}

public class TvCollectionService : ITvCollectionFetcher
{
    private readonly HttpClient _httpClient;

    public TvCollectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/tv/{season.Year}/{season.Season}",  cancellationToken);
        return await response.MapToObject<SeasonCollection>(new EmptySeasonCollection());
    }
}