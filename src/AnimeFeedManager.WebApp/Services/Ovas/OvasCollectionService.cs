using AnimeFeedManager.Features.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Ovas;

public interface IOvasCollectionService
{
    public Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default);
}

public sealed class OvasCollectionService : IOvasCollectionService
{
    private readonly HttpClient _httpClient;

    public OvasCollectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ShortSeasonCollection> GetSeasonLibrary(SimpleSeasonInfo season,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ovas/{season.Year}/{season.Season}", cancellationToken);
        return await response.MapToObject(ShortSeasonCollectionContext.Default.ShortSeasonCollection,
            new EmptyShortSeasonCollection());
    }
}