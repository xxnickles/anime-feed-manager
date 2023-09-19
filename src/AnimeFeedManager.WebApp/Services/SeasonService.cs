using System.Collections.Immutable;
using AnimeFeedManager.Features.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonFetcherService
{
    Task<ImmutableList<SimpleSeasonInfo>> GetAvailableSeasons(CancellationToken cancellationToken = default);
}

public sealed class SeasonService : ISeasonFetcherService
{
    private readonly HttpClient _httpClient;

    public SeasonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<SimpleSeasonInfo>> GetAvailableSeasons(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/seasons", cancellationToken);
        return await response.MapToList<SimpleSeasonInfo>();
    }
}