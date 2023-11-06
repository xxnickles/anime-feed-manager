using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Tv;

public interface ITvCollectionFetcher
{
    public Task<SeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default);
}

public class TvCollectionService(HttpClient httpClient) : ITvCollectionFetcher
{
    public async Task<SeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/tv/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject(SeasonCollectionContext.Default.SeasonCollection, new EmptySeasonCollection());
    }
}