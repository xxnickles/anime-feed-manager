using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.WebApp.Services;

namespace AnimeFeedManager.WebApp.State;

public sealed class SeasonSideEffects(ISeasonFetcherService seasonFetcherService)
{
    public async Task LoadAvailableSeasons(ApplicationState state, bool forceRefresh = false,
        CancellationToken token = default)
    {
        if (!state.Value.AvailableSeasons.Any() || forceRefresh)
        {
            const string key = "lo_seasons";
            try
            {
                state.AddLoadingItem(key, "Loading Season");
                var seasons = await seasonFetcherService.GetAvailableSeasons(token);
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