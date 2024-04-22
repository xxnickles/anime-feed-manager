using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISeasonFetcherService
{
    Task<ImmutableList<SimpleSeasonInfo>> GetAvailableSeasons(CancellationToken cancellationToken = default);
}

public sealed class SeasonService(HttpClient httpClient) : ISeasonFetcherService
{
    public async Task<ImmutableList<SimpleSeasonInfo>> GetAvailableSeasons(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/seasons", cancellationToken);

        var value = await response.MapToObject(SimpleSeasonInfoContext.Default.SimpleSeasonInfoArray, []);

        return value.ToImmutableList();
    }
}