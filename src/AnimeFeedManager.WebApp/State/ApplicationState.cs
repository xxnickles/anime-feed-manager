using System.Collections.Immutable;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.WebApp.Services;
using MudBlazor;

namespace AnimeFeedManager.WebApp.State
{
    public readonly record struct AppException(string Identifier, Exception Exception);

    public readonly record struct AppNotification(string Message, Severity Severity);


    public record State(
        SimpleSeasonInfo SelectedSeason,
        SeriesType SelectedSection,
        ImmutableList<SimpleSeasonInfo> AvailableSeasons,
        User User,
        HubConnectionStatus HubStatus,
        ImmutableList<string> TvSubscriptions,
        ImmutableList<string> TvInterested,
        ImmutableList<string> OvasSubscriptions,
        ImmutableList<string> MoviesSubscriptions,
        ImmutableDictionary<string, string> LoadingItems);


    public sealed class ApplicationState
    {
        /// <summary>
        /// The State property with initial value
        /// </summary>
        public State Value { get; private set; } = new(
            new NullSimpleSeasonInfo(),
            SeriesType.Tv,
            ImmutableList<SimpleSeasonInfo>.Empty,
            new AnonymousUser(),
            HubConnectionStatus.None,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty,
            ImmutableDictionary<string, string>.Empty
        );

        /// <summary>
        /// Notifies when selected season changes
        /// </summary>
        public event Func<SimpleSeasonInfo, ValueTask>? OnSelectedSeason;

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

        public async Task SetSelectedSeason(SimpleSeasonInfo season)
        {
            SetState(Value with {SelectedSeason = season});
            if (OnSelectedSeason != null) await OnSelectedSeason(season);
        }

        public void SetAvailableSeasons(ImmutableList<SimpleSeasonInfo> seasons)
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
            SetState(Value with {OvasSubscriptions = subscriptions});
        }

        public void AddOvaSubscription(string subscription)
        {
            SetState(Value with {OvasSubscriptions = Value.OvasSubscriptions.Add(subscription)});
        }

        public void RemoveOvaSubscription(string subscription)
        {
            SetState(Value with {OvasSubscriptions = Value.OvasSubscriptions.Remove(subscription)});
        }

        public void SetMoviesSubscriptions(ImmutableList<string> subscriptions)
        {
            SetState(Value with {MoviesSubscriptions = subscriptions});
        }

        public void AddMovieSubscription(string subscription)
        {
            SetState(Value with {MoviesSubscriptions = Value.MoviesSubscriptions.Add(subscription)});
        }

        public void RemoveMovieSubscription(string subscription)
        {
            SetState(Value with {MoviesSubscriptions = Value.MoviesSubscriptions.Remove(subscription)});
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
            SetState(Value with {HubStatus = status});
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
}