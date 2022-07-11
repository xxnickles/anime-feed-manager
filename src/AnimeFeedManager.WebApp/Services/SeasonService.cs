using System.Collections.ObjectModel;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonFetcherService
{
    Task<ReadOnlyCollection<SeasonInfoDto>> GetAvailableSeasons();
}

public class SeasonService : ISeasonFetcherService
{
    private readonly HttpClient _httpClient;

    public SeasonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReadOnlyCollection<SeasonInfoDto>> GetAvailableSeasons()
    {
        var results = await _httpClient.GetFromJsonAsync<List<SeasonInfoDto>>("api/seasons");
        return results?.AsReadOnly() ?? new List<SeasonInfoDto>().AsReadOnly();
    }
}