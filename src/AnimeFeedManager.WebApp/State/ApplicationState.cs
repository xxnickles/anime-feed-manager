using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.State;

public record State(
    SeasonInfoDto Season,
    User User
);

public class ApplicationState
{
    /// <summary>
    /// The State property with initial value
    /// </summary>
    public State Value { get; set; } = new(
        new NullSeasonInfo(),
        new AnonymousUser()
    );

    /// <summary>
    /// The event that will be raised for state changed
    /// </summary>
    public event Action? OnStateChange;

    /// <summary>
    /// The method that will be accessed by the sender component 
    /// to update the state
    /// </summary>
    public void SetSeason(SeasonInfoDto season)
    {
        Value = Value with {Season = season };
        NotifyStateChanged();
    }

    public void SetUser(User user)
    {
        Value = Value with {User = user};
        NotifyStateChanged();
    }

    /// <summary>
    /// The state change event notification
    /// </summary>
    private void NotifyStateChanged() => OnStateChange?.Invoke();
}