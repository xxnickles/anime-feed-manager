using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.State;

public record State(
    SeasonInfoDto Season,
    User User,
    ImmutableList<string> Subscriptions,
    ImmutableList<string> Interested,
    ImmutableDictionary<string, string> LoadingItems);

public class ApplicationState
{
    /// <summary>
    /// The State property with initial value
    /// </summary>
    public State Value { get; set; } = new(
        new NullSeasonInfo(),
        new AnonymousUser(),
        ImmutableList<string>.Empty,
        ImmutableList<string>.Empty,
        ImmutableDictionary<string, string>.Empty
    );

    /// <summary>
    /// The event that will be raised for state changed
    /// </summary>
    public event Action? OnStateChange;
    
    /// <summary>
    /// The event that will be raised for state changed
    /// </summary>
    public event Func<User, Task>? OnUserChanges;

    public void SetSeason(SeasonInfoDto season)
    {
        Value = Value with {Season = season};
        NotifyStateChanged();
    }

    public void SetUser(User user)
    {
        Value = Value with {User = user};
        NotifyStateChanged();
        OnUserChanges?.Invoke(user);

    }

    public void SetSubscriptions(ImmutableList<string> subscriptions)
    {
        Value = Value with {Subscriptions = subscriptions};
        NotifyStateChanged();
    }

    public void SetInterested(ImmutableList<string> interested)
    {
        Value = Value with {Interested = interested};
        NotifyStateChanged();
    }

    public void AddInterested(string interested)
    {
        Value = Value with {Interested = Value.Interested.Add(interested)};
        NotifyStateChanged();
    }

    public void RemoveInterested(string interested)
    {
        Value = Value with {Interested = Value.Interested.Remove(interested)};
        NotifyStateChanged();
    }


    public void AddSubscription(string subscription)
    {
        Value = Value with {Subscriptions = Value.Subscriptions.Add(subscription)};
        NotifyStateChanged();
    }

    public void RemoveSubscription(string subscription)
    {
        Value = Value with {Subscriptions = Value.Subscriptions.Remove(subscription)};
        NotifyStateChanged();
    }

    public void AddLoadingItem(string key, string description)
    {
        if (Value.LoadingItems.ContainsKey(key)) return;
        Value = Value with {LoadingItems = Value.LoadingItems.Add(key, description)};
        NotifyStateChanged();
    }

    public void RemoveLoadingItem(string key)
    {
        if (!Value.LoadingItems.ContainsKey(key)) return;
        Value = Value with {LoadingItems = Value.LoadingItems.Remove(key)};
        NotifyStateChanged();
    }

    /// <summary>
    /// The state change event notification
    /// </summary>
    private void NotifyStateChanged() => OnStateChange?.Invoke();
}