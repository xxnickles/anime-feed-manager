using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Ovas;

public interface IOvasCollectionService
{
    public Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default);
}

public sealed class OvasCollectionService(HttpClient httpClient) : IOvasCollectionService
{
    public async Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/ovas/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject(ShortSeasonCollectionContext.Default.ShortSeasonCollection, new EmptyShortSeasonCollection());
    }
}