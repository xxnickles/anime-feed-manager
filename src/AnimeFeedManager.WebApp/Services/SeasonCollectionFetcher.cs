using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonCollectionFetcher
{
    public Task<SeasonCollection> GetSeasonLibrary(SeasonInfoDto season);
}

public class SeasonCollectionFetcher : ISeasonCollectionFetcher
{
    private readonly HttpClient _httpClient;

    public SeasonCollectionFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SeasonCollection> GetSeasonLibrary(SeasonInfoDto season)
    {
        return await _httpClient.GetFromJsonAsync<SeasonCollection>($"api/library/{season.Year}/{season.Season}") ?? new EmptySeasonCollection();
    }
}