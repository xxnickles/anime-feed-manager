namespace AnimeFeedManager.Common.Domain.Notifications.Base;

public enum NotificationAreaValue {
    None,
    Feed,
    Update,
    Error
}

public readonly record struct NotificationArea
{
    private const string FeedValue = "FEED";
    private const string UpdateValue = "UPDATE";
    private const string ErrorValue = "ERROR";
    private const string NoneValue = "NONE";
    
    public string Value { get; }
    public NotificationAreaValue Type {get;}

    private NotificationArea(string value, NotificationAreaValue type)
    {
        Value = value;
        Type = type;
    }

    public static NotificationArea Feed = new(FeedValue, NotificationAreaValue.Feed);
    public static NotificationArea Update = new(UpdateValue, NotificationAreaValue.Update);
    public static NotificationArea Error = new(ErrorValue, NotificationAreaValue.Error);
    public static NotificationArea None = new(NoneValue, NotificationAreaValue.None);
   
    public static implicit operator string(NotificationArea area) => area.Value;

    public static NotificationArea Parse(string value)
    {
        return value switch
        {
            FeedValue => Feed,
            UpdateValue => Update,
            ErrorValue => Error,
            _ => None
        };
    }
}