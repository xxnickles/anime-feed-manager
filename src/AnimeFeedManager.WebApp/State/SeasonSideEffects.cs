using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.WebApp.Services;

namespace AnimeFeedManager.WebApp.State;

public sealed class SeasonSideEffects
{
    private readonly ISeasonFetcherService _seasonFetcherService;

    public SeasonSideEffects(
        ISeasonFetcherService seasonFetcherService)
    {
        _seasonFetcherService = seasonFetcherService;
    }

    public async Task LoadAvailableSeasons(ApplicationState state, bool forceRefresh = false,
        CancellationToken token = default)
    {
        if (!state.Value.AvailableSeasons.Any() || forceRefresh)
        {
            const string key = "lo_seasons";
            try
            {
                state.AddLoadingItem(key, "Loading Season");
                var seasons = await _seasonFetcherService.GetAvailableSeasons(token);
                if (seasons.Count > 0)
                {
                    var latest = seasons[0];
                    state.SetAvailableSeasons(seasons);
                    if (latest is not NullSimpleSeasonInfo)
                    {
                        await state.SetSelectedSeason(latest);
                    }
                }

                state.RemoveLoadingItem(key);
            }
            catch (Exception e)
            {
                state.ReportException(new AppException("Season Fetching", e));
                state.RemoveLoadingItem(key);
            }
        }
    }
}