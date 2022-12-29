using System.Collections.Immutable;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.WebApp.Services;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State;

public record struct AppException(string Identifier, Exception Exception);

public record struct AppNotification(string Message, Severity Severity);

// Using array as trimming (for serialization purposes) is not so nice with immutable lists. We have no control of the local storage library serialization
// It is just simple to use a native primitive just for the sake of it.
// https://github.com/dotnet/runtime/issues/62242
public record LocalStorageState(SeasonInfoDto[] AvailableSeasons, long Stamp)
{
    public static implicit operator State(LocalStorageState localStorageState) =>
        new(
            localStorageState.AvailableSeasons.Any() ? localStorageState.AvailableSeasons[0] : new NullSeasonInfo(),
            SeriesType.Tv,
            localStorageState.AvailableSeasons.ToImmutableList(),
            new AnonymousUser(),
            HubConnectionStatus.None,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableDictionary<string, string>.Empty);
}

public record State(
    SeasonInfoDto SelectedSeason,
    SeriesType SelectedSection,
    ImmutableList<SeasonInfoDto> AvailableSeasons,
    User User,
    HubConnectionStatus HubStatus,
    ImmutableList<string> TvSubscriptions,
    ImmutableList<string> TvInterested,
    ImmutableList<string> OvasSubscriptions,
    ImmutableDictionary<string, string> LoadingItems)
{
    public static implicit operator LocalStorageState(State state) =>
        new(state.AvailableSeasons.ToArray(), DateTime.UtcNow.Ticks);
}

public sealed class ApplicationState
{
    /// <summary>
    /// The State property with initial value
    /// </summary>
    public State Value { get; private set; } = new(
        new NullSeasonInfo(),
        SeriesType.Tv,
        ImmutableList<SeasonInfoDto>.Empty,
        new AnonymousUser(),
        HubConnectionStatus.None,
        ImmutableList<string>.Empty,
        ImmutableList<string>.Empty,
        ImmutableList<string>.Empty,
        ImmutableDictionary<string, string>.Empty
    );

    /// <summary>
    /// Notifies when selected season changes
    /// </summary>
    public event Func<SeasonInfoDto, ValueTask>? OnSelectedSeason;

    /// <summary>
    /// Notifies when Exceptions Happen
    /// </summary>
    public event Action<AppException>? OnException;

    /// <summary>
    /// Notifies when Notifications Happen
    /// </summary>
    public event Action<AppNotification>? OnNotification;

    /// <summary>
    /// The event that will be raised for state changed
    /// </summary>
    public event Action? OnStateChange;

    /// <summary>
    /// Event that will be raised for user state changed
    /// </summary>
    public event Func<User, Task>? OnUserChanges;

    public void SetState(State newState)
    {
        Value = newState;
        OnStateChange?.Invoke();
    }

    public async Task SetSelectedSeason(SeasonInfoDto season)
    {
        SetState(Value with {SelectedSeason = season});
        if (OnSelectedSeason != null) await OnSelectedSeason(season);
    }

    public void SetAvailableSeasons(ImmutableList<SeasonInfoDto> seasons)
    {
        SetState(Value with {AvailableSeasons = seasons});
    }

    public void SetUser(User user)
    {
        SetState(Value with {User = user});
        OnUserChanges?.Invoke(user);
    }

    public void SetSubscriptions(ImmutableList<string> subscriptions)
    {
        SetState(Value with {TvSubscriptions = subscriptions});
    }

    public void SetInterested(ImmutableList<string> interested)
    {
        SetState(Value with {TvInterested = interested});
    }

    public void AddInterested(string interested)
    {
        SetState(Value with {TvInterested = Value.TvInterested.Add(interested)});
    }


    public void RemoveInterested(string interested)
    {
        SetState(Value with {TvInterested = Value.TvInterested.Remove(interested)});
    }


    public void AddSubscription(string subscription)
    {
        SetState(Value with {TvSubscriptions = Value.TvSubscriptions.Add(subscription)});
    }

    public void RemoveSubscription(string subscription)
    {
        SetState(Value with {TvSubscriptions = Value.TvSubscriptions.Remove(subscription)});
    }
    
    public void SetOvasSubscriptions(ImmutableList<string> subscriptions)
    {
        SetState(Value with {TvSubscriptions = subscriptions});
    }
    
    public void AddOvaSubscription(string subscription)
    {
        SetState(Value with {TvSubscriptions = Value.OvasSubscriptions.Add(subscription)});
    }

    public void RemoveOvaSubscription(string subscription)
    {
        SetState(Value with {TvSubscriptions = Value.OvasSubscriptions.Remove(subscription)});
    }

    public void AddLoadingItem(string key, string description)
    {
        if (Value.LoadingItems.ContainsKey(key)) return;
        SetState(Value with {LoadingItems = Value.LoadingItems.Add(key, description)});
    }

    public void RemoveLoadingItem(string key)
    {
        if (!Value.LoadingItems.ContainsKey(key)) return;
        SetState(Value with {LoadingItems = Value.LoadingItems.Remove(key)});
    }

    public void SetHubStatus(HubConnectionStatus status)
    {
        SetState(Value with{ HubStatus = status});
    }

    /// <summary>
    /// Notifies application exceptions
    /// </summary>
    /// <param name="ex"></param>
    public void ReportException(AppException ex)
    {
        OnException?.Invoke(ex);
    }

    /// <summary>
    /// Notifications event trigger
    /// </summary>
    /// <param name="notification"></param>
    public void ReportNotification(AppNotification notification)
    {
        OnNotification?.Invoke(notification);
    }
}