using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using Blazored.LocalStorage;

namespace AnimeFeedManager.WebApp.State;

public sealed class LocalStorageSideEffects
{
    private readonly ILocalStorageService _localStorage;
    private const string StateKey = "state";

    public LocalStorageSideEffects(
        ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task TryAssignLocalState(ApplicationState state)
    {
        if (await _localStorage.ContainKeyAsync(StateKey))
        {
            try
            {
                var localState = await _localStorage.GetItemAsync<LocalStorageState>(StateKey);
                state.SetState(localState);
                await state.SetSelectedSeason(localState.AvailableSeasons.Any()
                    ? localState.AvailableSeasons[0]
                    : new NullSeasonInfo());
            }
            catch (Exception e)
            {
                state.SetState(new State(
                    new NullSeasonInfo(),
                    ImmutableList<SeasonInfoDto>.Empty,
                    new AnonymousUser(),
                    ImmutableList<string>.Empty,
                    ImmutableList<string>.Empty,
                    ImmutableDictionary<string, string>.Empty
                ));
                await state.SetSelectedSeason(new NullSeasonInfo());
                state.ReportException(new AppException("Local User", e));
            }
        }
    }

    public ValueTask StoreState(ApplicationState state)
    {
        return _localStorage.SetItemAsync<LocalStorageState>(StateKey, state.Value);
    }
}