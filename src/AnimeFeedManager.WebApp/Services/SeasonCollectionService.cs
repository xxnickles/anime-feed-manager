using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonCollectionFetcher
{
    public Task<SeasonCollection> GetSeasonLibrary(SeasonInfoDto season, CancellationToken cancellationToken = default);
}

public class SeasonCollectionService : ISeasonCollectionFetcher
{
    private readonly HttpClient _httpClient;

    public SeasonCollectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SeasonCollection> GetSeasonLibrary(SeasonInfoDto season, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/library/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject<SeasonCollection>(new EmptySeasonCollection());
    }
}