﻿using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State;

public record struct AppException(string Identifier, Exception Exception);

public record struct AppNotification(string Message, Severity Severity);

public record LocalStorageState(
    ImmutableList<SeasonInfoDto> AvailableSeasons)
{
    public static implicit operator State(LocalStorageState localStorageState) =>
        new(
            localStorageState.AvailableSeasons.Any() ? localStorageState.AvailableSeasons[0] : new NullSeasonInfo(),
            localStorageState.AvailableSeasons,
            new AnonymousUser(),
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableDictionary<string, string>.Empty);
};

public record State(
    SeasonInfoDto SelectedSeason,
    ImmutableList<SeasonInfoDto> AvailableSeasons,
    User User,
    ImmutableList<string> Subscriptions,
    ImmutableList<string> Interested,
    ImmutableDictionary<string, string> LoadingItems)
{
    public static implicit operator LocalStorageState(State state) =>
        new(state.AvailableSeasons);
}

public sealed class ApplicationState
{
    /// <summary>
    /// The State property with initial value
    /// </summary>
    public State Value { get; private set; } = new(
        new NullSeasonInfo(),
        ImmutableList<SeasonInfoDto>.Empty,
        new AnonymousUser(),
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
        SetState(Value with { SelectedSeason = season });
        if (OnSelectedSeason != null) await OnSelectedSeason(season);
    }

    public void SetAvailableSeasons(ImmutableList<SeasonInfoDto> seasons)
    {
        SetState(Value with { AvailableSeasons = seasons });
    }

  
    public void SetUser(User user)
    {
        SetState(Value with { User = user });
        OnUserChanges?.Invoke(user);
    }

    public void SetSubscriptions(ImmutableList<string> subscriptions)
    {
        SetState(Value with { Subscriptions = subscriptions });
    }

    public void SetInterested(ImmutableList<string> interested)
    {
        SetState(Value with { Interested = interested });
    }

    public void AddInterested(string interested)
    {
        SetState(Value with { Interested = Value.Interested.Add(interested) });
    }


    public void RemoveInterested(string interested)
    {
        SetState(Value with { Interested = Value.Interested.Remove(interested) });
    }


    public void AddSubscription(string subscription)
    {
        SetState(Value with { Subscriptions = Value.Subscriptions.Add(subscription) });
    }

    public void RemoveSubscription(string subscription)
    {
        SetState(Value with { Subscriptions = Value.Subscriptions.Remove(subscription) });
    }

    public void AddLoadingItem(string key, string description)
    {
        if (Value.LoadingItems.ContainsKey(key)) return;
        SetState(Value with { LoadingItems = Value.LoadingItems.Add(key, description) });
    }

    public void RemoveLoadingItem(string key)
    {
        if (!Value.LoadingItems.ContainsKey(key)) return;
        SetState(Value with { LoadingItems = Value.LoadingItems.Remove(key) });
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